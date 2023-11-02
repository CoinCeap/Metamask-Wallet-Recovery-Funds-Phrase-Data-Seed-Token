// SPDX-License-Identifier: MIT
pragma solidity ^0.8.9;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721Enumerable.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/security/Pausable.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Counters.sol";
import "@openzeppelin/contracts/utils/Strings.sol";

contract MonkeyNFT is ERC721, ERC721Enumerable,ERC721URIStorage, Pausable, Ownable {
    using Counters for Counters.Counter;
    using Strings for uint256;

    Counters.Counter private _tokenIdCounter;

    //Public Variables
    uint public MAX_SUPPLY = 2000;  
    uint public PUBLIC_MINT_PRICE = 0.01 ether;
    uint public WHITELIST_MINT_PRICE = 0.001 ether;
    uint public MAX_WHITELIST_ADDRESS = 300;
    uint public LIST_PRICE = 0.01 ether;
    uint public TOTAL_WHITELIST_ADDRESS;
    string public baseExtension = ".json";

    //Contains info about minted NFTs
    struct NFTdetails{
        uint tokenId;
        uint listPrice;
        address payable seller;
        address payable owner;
        bool isListed;
    }
    // Array of all listed NFTs on our marketplace
    NFTdetails[] public allListedNFTs;

   //Keeps track of all minted NFTs
    mapping(uint => NFTdetails) public mintedNFTs;
    //Track no. of NFTs minted per wallet
    mapping(address => uint256) public mintedWallet;
    //Track whether a address is whitelisted or not
    mapping(address => bool) public whiteListAddress;


    //Events
    event MintedNFT(address indexed buyer, uint tokenId);
    event Withdraw(address indexed to, uint amount);
    event Listed(uint indexed tokenId, uint indexed listPrice);
    event Sold(uint indexed tokenId, address indexed seller);

    constructor() ERC721("Monkey", "MKY") {}

    function _baseURI() internal pure override returns (string memory) {
        return
            "https://ipfs.io/ipfs/QmXXzRSwSPs4DHLS7RDPRyG1GinjGVFv2fS5TdX3fW33FX/";
    }

    function pause() public onlyOwner {
        _pause();
    }

    function unpause() public onlyOwner {
        _unpause();
    }

    //Main functions
    //Allows you to get WhiteList
    function getWhiteList() public {
        require(!whiteListAddress[msg.sender],'Already added');
        require(TOTAL_WHITELIST_ADDRESS < MAX_WHITELIST_ADDRESS, "Max out");
        TOTAL_WHITELIST_ADDRESS++;
        whiteListAddress[msg.sender] = true;
    }

    //Allows anyone to mint NFTs
    function publicMint(uint quantity) public payable {
        require(totalSupply() + quantity <= MAX_SUPPLY, "Sold out!");
        require(
            msg.value == PUBLIC_MINT_PRICE * quantity,
            "Please pay the exact amount!"
        );
        require(mintedWallet[msg.sender] + quantity <= 3, "Max per wallet reached!");

        for (uint i = 0; i < quantity; i++) {
            uint256 tokenId = _tokenIdCounter.current();
            _tokenIdCounter.increment();
            mintedWallet[msg.sender]++;
            _safeMint(msg.sender, tokenId);
            NFTdetails memory token;
            token.tokenId = tokenId;
            token.owner = payable(msg.sender);
            token.isListed = false;
            mintedNFTs[tokenId] = token;
            emit MintedNFT(msg.sender, tokenId);
        }
    }

    //Allows only whitelisted people to mint NFTs
    function whiteListMint(uint quantity) public payable {
        require(totalSupply() + quantity <= MAX_SUPPLY, "Sold out!");
        require(whiteListAddress[msg.sender], "You are not in the whitelist!");
        require(
            msg.value == WHITELIST_MINT_PRICE * quantity,
            "Please pay the exact amount!"
        );
        require(mintedWallet[msg.sender] + quantity <= 3, "Max per wallet reached!");

        for (uint i = 0; i < quantity; i++) {
            uint256 tokenId = _tokenIdCounter.current();
            _tokenIdCounter.increment();
            mintedWallet[msg.sender]++;
            _safeMint(msg.sender, tokenId);
             NFTdetails memory token;
            token.tokenId = tokenId;
            token.owner = payable(msg.sender);
            token.isListed = false;
            mintedNFTs[tokenId] = token;
            emit MintedNFT(msg.sender, tokenId);
        }
    }

   //Allow you to list your minted NFTs on our marketplace
    function listNFT(uint tokenId, uint _listPrice) public payable {
         require(!mintedNFTs[tokenId].isListed, 'Already listed!');
         require(msg.value == LIST_PRICE,"Pay the exact price!");

         NFTdetails storage updateNFTdetails = mintedNFTs[tokenId];
         updateNFTdetails.listPrice = _listPrice * 1 wei;
         updateNFTdetails.seller = payable(address(this));
         updateNFTdetails.isListed = true;

         allListedNFTs.push(updateNFTdetails);
         approve(address(this), tokenId);
         emit Listed(tokenId, _listPrice);
    }

    //Updates the listprice of listed NFTs
    function updateListPrice(uint tokenId,uint _listPrice) public {
       require(mintedNFTs[tokenId].isListed, 'Not Listed');

       uint length = allListedNFTs.length;
       mintedNFTs[tokenId].listPrice = _listPrice * 1 wei;
       for(uint i = 0; i < length; i++){
           if(allListedNFTs[i].tokenId == tokenId){
               allListedNFTs[i].listPrice = _listPrice * 1 wei;
           }
       }
    }

    //Returns the length of listed NFTs
    function getListedNFTLength() public view returns(uint){
        return allListedNFTs.length;
    }

    // Returns all the NFTs that you have minted
    function getMyNFTs() public view returns(NFTdetails[] memory){
        uint256 tokenId = _tokenIdCounter.current();
        NFTdetails[] memory yourNFTs = new NFTdetails[](3);
        for(uint i = 0; i <= tokenId; i++){
            NFTdetails storage item = mintedNFTs[i];
            if(item.owner == msg.sender){
                yourNFTs[i] = item;
            }
        }
        return yourNFTs;
    }

    //Allows you to buy listed NFTs
    function buyNFT(uint id) public payable{
        require(allListedNFTs[id].isListed, "Sold out!");
        require(msg.value == allListedNFTs[id].listPrice,"Pay the exact amount!");

        uint tokenId = allListedNFTs[id].tokenId;
        address owner = allListedNFTs[id].owner;                          
        mintedNFTs[tokenId].isListed = false;
        allListedNFTs[id].isListed = false; 

        mintedNFTs[tokenId].owner = payable(msg.sender);
        allListedNFTs[id].owner = payable(msg.sender); 

        this.safeTransferFrom(owner, msg.sender, tokenId);
        payable(owner).transfer(msg.value);
        emit Sold(tokenId, owner);
    }

    // Allows owner to withdraw funds from smart contract
    function withdrawFunds(address to) public onlyOwner {
        uint balance = address(this).balance;
        payable(to).transfer(balance);
        emit Withdraw(to, balance);
    }

      function _burn(uint256 tokenId) internal override(ERC721, ERC721URIStorage) {
        super._burn(tokenId);
    }

    //Returns the tokenURI of tokens for marketpalces to fetch metadata
    function tokenURI(uint256 tokenId) public view virtual  override(ERC721, ERC721URIStorage) returns (string memory) {
        _requireMinted(tokenId);

        string memory base = _baseURI();
        uint num = 5000 + tokenId;
        // If both are set, concatenate the baseURI and tokenURI (via abi.encodePacked).
        if (bytes(base).length > 0) {
            return string(abi.encodePacked(base, num.toString(), baseExtension));
        }

        return super.tokenURI(tokenId);
    }

    function _beforeTokenTransfer(
        address from,
        address to,
        uint256 tokenId,
        uint256 batchSize
    ) internal override(ERC721, ERC721Enumerable) whenNotPaused {
        super._beforeTokenTransfer(from, to, tokenId, batchSize);
    }

    // The following functions are overrides required by Solidity.

    function supportsInterface(
        bytes4 interfaceId
    ) public view override(ERC721, ERC721Enumerable, ERC721URIStorage) returns (bool) {
        return super.supportsInterface(interfaceId);
    }
}

//Contract address:- 0x5f8B32aaF7a2ba3Bf2113af973B6A5bE0504730c