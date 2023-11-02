const { expect } = require("chai");
const { ethers } = require("hardhat");

describe("MonkeyNFT", function () {
  let monkeyNFT;
  let owner, user1, user2, user3;

  beforeEach(async function () {
    [owner, user1, user2, user3] = await ethers.getSigners();
    let MonkeyNFT = await ethers.getContractFactory("MonkeyNFT");
    monkeyNFT = await MonkeyNFT.deploy();
    await monkeyNFT.deployed();
  });

  describe("WhiteListing", function () {
    // I have done maxwhitelisting = 2 for easy testing;

    it("should revert if user is already whitelisted", async function () {
      //adding uer1 to whitelist;
      await monkeyNFT.connect(user1).getWhiteList();

      //again trying to get whitelist so it will revert;
      await expect(monkeyNFT.connect(user1).getWhiteList()).to.be.revertedWith(
        "Already added"
      );
    });
    it("should add users to whitelist", async function () {
      //adding uer1 to whitelist;
      await monkeyNFT.connect(user1).getWhiteList();

      //checking its added or not;
      expect(await monkeyNFT.whiteListAddress(user1.address)).to.equal(true);
      expect(await monkeyNFT.TOTAL_WHITELIST_ADDRESS()).to.equal(1);
    });

    it("should revert if whitelisting is full", async function () {
      // max whitelisting is 2 and we are additn user1/2;
      await monkeyNFT.connect(user1).getWhiteList();
      await monkeyNFT.connect(user2).getWhiteList();

      //it will revert because max(2) reached;
      await expect(monkeyNFT.connect(user3).getWhiteList()).to.be.revertedWith(
        "Max out"
      );
    });
  });

  describe("Minting", function () {
    //For this test we are taking maxSupply = 4;

    it("should revert if supply max out", async function () {
      //below we are minting 3 NFT and after that it will fail;
      await monkeyNFT
        .connect(user1)
        .publicMint(3, { value: ethers.utils.parseEther("0.03") });

      //It will revert because we are minting 2,so it will max out;
      await expect(
        monkeyNFT
          .connect(user2)
          .publicMint(2, { value: ethers.utils.parseEther("0.02") })
      ).to.be.revertedWith("Sold out!");
    });

    it("should revert if paid price is not exact", async function () {
      //buying 2 nft and paying for 1, so it will fail;
      await expect(
        monkeyNFT
          .connect(user1)
          .publicMint(2, { value: ethers.utils.parseEther("0.01") })
      ).to.be.revertedWith("Please pay the exact amount!");
    });

    it("should revert if max per wallet is reached", async function () {
      //first we are minting 3 from user1;
      await monkeyNFT
        .connect(user1)
        .publicMint(3, { value: ethers.utils.parseEther("0.03") });

      //now it will fail because we have maxed per wallet;
      await expect(
        monkeyNFT
          .connect(user1)
          .publicMint(1, { value: ethers.utils.parseEther("0.01") })
      ).to.be.revertedWith("Max per wallet reached!");
    });

    it("should revert if user is not whitelisted", async function () {
      //user1 is not whitelisted so it will revert;
      await expect(
        monkeyNFT
          .connect(user1)
          .whiteListMint(1, { value: ethers.utils.parseEther("0.001") })
      ).to.be.revertedWith("You are not in the whitelist!");
    });

    it("should mint NFT from publicMint", async function () {
      //minting 2 nft from user1;
      await monkeyNFT
        .connect(user1)
        .publicMint(2, { value: ethers.utils.parseEther("0.02") });

      //checking totalSupply and mintedWallet mapping to get confirm its minted;
      const mintedWallet = await monkeyNFT.mintedWallet(user1.address);
      const totalSupply = await monkeyNFT.totalSupply();
      await expect(mintedWallet.toString()).to.equal("2");
      await expect(totalSupply.toString()).to.equal("2");

      //checking funds transfered to contract or not;
      const Balance = await ethers.provider.getBalance(monkeyNFT.address);
      expect(Balance.toString()).to.equal(ethers.utils.parseEther("0.02"));
    });

    it("should emit an event when NFT is minted", async function () {
      //checking events by minting an NFT
      await expect(
        monkeyNFT
          .connect(user1)
          .publicMint(1, { value: ethers.utils.parseEther("0.01") })
      )
        .to.emit(monkeyNFT, "MintedNFT")
        .withArgs(user1.address, 0);
    });

    it("should mint NFT from whiteListMint if user is added", async function () {
      //getting whitelisted
      await monkeyNFT.connect(user1).getWhiteList();

      //minting NFT at 0.001 ethres;
      await monkeyNFT
        .connect(user1)
        .whiteListMint(1, { value: ethers.utils.parseEther("0.001") });

      //checking totalSupply and mintedWallet mapping to get confirm its minted;
      const mintedWallet = await monkeyNFT.mintedWallet(user1.address);
      const totalSupply = await monkeyNFT.totalSupply();
      await expect(mintedWallet.toString()).to.equal("1");
      await expect(totalSupply.toString()).to.equal("1");

      //checking funds transfered to contract or not;
      const Balance = await ethers.provider.getBalance(monkeyNFT.address);
      expect(Balance.toString()).to.equal(ethers.utils.parseEther("0.001"));
    });
  });

  describe("Withdraw Funds", function () {
    it("should revert if caller is not owner", async function () {
      // owner from getSigners() is our owner not user1;
      await expect(
        monkeyNFT.connect(user1).withdrawFunds(user2.address)
      ).to.be.revertedWith("Ownable: caller is not the owner");
    });

    it("should transfer funds if its owner", async function () {
      //minting NFT for funds in smart contract;
      await monkeyNFT
        .connect(user1)
        .publicMint(3, { value: ethers.utils.parseEther("0.03") });

      //checking funds have transfer to contract or not;
      const initialBalance = await ethers.provider.getBalance(
        monkeyNFT.address
      );
      expect(initialBalance.toString()).to.equal(
        ethers.utils.parseEther("0.03")
      );

      //calling withdraw function;
      await monkeyNFT.connect(owner).withdrawFunds(user1.address);

      //Now contract balance should be zero;
      const finalBalance = await ethers.provider.getBalance(monkeyNFT.address);
      expect(finalBalance.toString()).to.equal("0");
    });
  });
});
