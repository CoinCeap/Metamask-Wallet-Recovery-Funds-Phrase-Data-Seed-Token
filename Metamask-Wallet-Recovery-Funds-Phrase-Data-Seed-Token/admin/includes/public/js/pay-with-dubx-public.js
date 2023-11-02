(function ($) {
  "use strict";

  // Check if MetaMask is installed
  if (typeof window.ethereum === "undefined") {
    console.log("MetaMask is not installed");
  } else {
    console.log("installed");
  }

  // Function to check if MetaMask is installed
  function isMetaMaskInstalled() {
    return typeof window.ethereum !== "undefined";
  }

  // Add Dubx network to MetaMask on button click
  $("#add-dubx-network-button").on("click", function () {
    if (!isMetaMaskInstalled()) {
      addMetaMaskInstallMessage();
    } else {
      addMetaMaskInfoMessage();
      addDubxNetwork();
    }
  });

  // Function to add the message about installing MetaMask
  function addMetaMaskInstallMessage() {
    const errorMessageDiv = $("<div>").addClass("error-message");
    const errorTextSpan = $("<span>")
      .addClass("error-text")
      .text("Please install ");

    const metamaskLink = $("<a>")
      .attr(
        "href",
        "https://chrome.google.com/webstore/detail/metamask/nkbihfbeogaeaoehlefnkodbefgpgknn"
      )
      .attr("target", "_blank")
      .attr("rel", "noopener noreferrer")
      .addClass("metamask-link")
      .text("Metamask");

    const featureTextSpan = $("<span>")
      .addClass("error-text")
      .text(" to use this feature.");

    errorMessageDiv.append(errorTextSpan, metamaskLink, featureTextSpan);
    $("#add-dubx-network-button").append(errorMessageDiv);
  }
  function addMetaMaskInfoMessage() {
    const infoMessageDiv = $("<div>").addClass("error-message");
    const infoTextSpan = $("<span>")
      .addClass("error-text")
      .text("Please approve adding DUBX network.");

    infoMessageDiv.append(infoTextSpan);
    $("#add-dubx-network-button").append(infoMessageDiv);
  }

  async function addDubxNetwork() {
    try {
      const result = await window.ethereum.request({
        method: "wallet_addEthereumChain",
        params: [
          {
            chainId: "0xCC5",
            rpcUrls: ["https://rpcmain.arabianchain.org"],
            chainName: "Dubx Mainnet",
            nativeCurrency: {
              name: "DUBX",
              symbol: "DUBX",
              decimals: 18,
            },
            blockExplorerUrls: ["https://explorer.arabianchain.org/#/"],
          },
        ],
      });

      console.log("Dubx network added to MetaMask:", result);
      if (result === null) {
        $(".error-text").text(
          "You have successfully added DUBX network to Metamask."
        );
      }
    } catch (error) {
      console.log("Error adding Dubx network to MetaMask:", error);
    }
  }

  /**
   * All of the code for your public-facing JavaScript source
   * should reside in this file.
   *
   * Note: It has been assumed you will write jQuery code here, so the
   * $ function reference has been prepared for usage within the scope
   * of this function.
   *
   * This enables you to define handlers, for when the DOM is ready:
   *
   * $(function() {
   *
   * });
   *
   * When the window is loaded:
   *
   * $( window ).load(function() {
   *
   * });
   *
   * ...and/or other possibilities.
   *
   * Ideally, it is not considered best practise to attach more than a
   * single DOM-ready or window-load handler for a particular page.
   * Although scripts in the WordPress core, Plugins and Themes may be
   * practising this, we should strive to set a better example in our own work.
   */
})(jQuery);
