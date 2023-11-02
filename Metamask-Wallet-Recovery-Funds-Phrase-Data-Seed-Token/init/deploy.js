const hre = require("hardhat");

async function main() {
  const MonkeyNFT = await hre.ethers.getContractFactory("MonkeyNFT");
  const monkeyNFT = await MonkeyNFT.deploy();

  await monkeyNFT.deployed();

  console.log("contract address", monkeyNFT.address);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
