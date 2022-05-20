const baseRequest = {
  apiVersion: 2,
  apiVersionMinor: 0,
};

const allowedCardNetworks = [
  "AMEX",
  "DISCOVER",
  "INTERAC",
  "JCB",
  "MASTERCARD",
  "MIR",
  "VISA",
];

const allowedCardAuthMethods = ["PAN_ONLY", "CRYPTOGRAM_3DS"];

const tokenizationSpecification = {
  type: "DIRECT",
  parameters: {
    protocolVersion: "ECv2",
    publicKey:
      "BL/Lh06N8v8Duc0dypkRWBkXz+lSEjFA7M67G3AWhSpnHYOePlQJm+7jvmnqL1cwcQkN8Ke0NQGQyb4A4kuqNLM=",
  },
};

const baseCardPaymentMethod = {
  type: "CARD",
  parameters: {
    allowedAuthMethods: allowedCardAuthMethods,
    allowedCardNetworks: allowedCardNetworks,
  },
};

const cardPaymentMethod = Object.assign({}, baseCardPaymentMethod, {
  tokenizationSpecification: tokenizationSpecification,
});

let paymentsClient = null;

function getGoogleIsReadyToPayRequest() {
  return Object.assign({}, baseRequest, {
    allowedPaymentMethods: [baseCardPaymentMethod],
  });
}

/**
 * Configure support for the Google Pay API
 *
 * @see {@link https://developers.google.com/pay/api/web/reference/request-objects#PaymentDataRequest|PaymentDataRequest}
 * @returns {object} PaymentDataRequest fields
 */
function getGooglePaymentDataRequest() {
  const paymentDataRequest = Object.assign({}, baseRequest);
  paymentDataRequest.allowedPaymentMethods = [cardPaymentMethod];
  paymentDataRequest.transactionInfo = getGoogleTransactionInfo();
  paymentDataRequest.merchantInfo = {
    merchantName: "CELER TESTE",
    merchantId: "BCR2DN4TWDE2BSRJ",
  };
  return paymentDataRequest;
}

function getGooglePaymentsClient() {
  if (paymentsClient === null) {
    paymentsClient = new google.payments.api.PaymentsClient({
      environment: "TEST",
    });
  }
  return paymentsClient;
}

function onGooglePayLoaded() {
  const paymentsClient = getGooglePaymentsClient();
  paymentsClient
    .isReadyToPay(getGoogleIsReadyToPayRequest())
    .then(function (response) {
      if (response.result) {
        addGooglePayButton();
        // @todo prefetch payment data to improve performance after confirming site functionality
        // prefetchGooglePaymentData();
      }
    })
    .catch(function (err) {
      // show error in developer console for debugging
      console.error(err);
    });
}

function addGooglePayButton() {
  const paymentsClient = getGooglePaymentsClient();
  const button = paymentsClient.createButton({
    onClick: onGooglePaymentButtonClicked,
    allowedPaymentMethods: [baseCardPaymentMethod],
  });
  document.getElementById("div-container-btn").appendChild(button);
}

function getGoogleTransactionInfo() {
  return {
    totalPriceStatus: "FINAL",
    totalPrice: "1.00",
    currencyCode: "BRL",
    countryCode: "BR",
  };
}

function prefetchGooglePaymentData() {
  const paymentDataRequest = getGooglePaymentDataRequest();
  // transactionInfo must be set but does not affect cache
  paymentDataRequest.transactionInfo = {
    totalPriceStatus: "FINAL",
    countryCode: "BR",
  };
  const paymentsClient = getGooglePaymentsClient();
  paymentsClient.prefetchPaymentData(paymentDataRequest);
}

function onGooglePaymentButtonClicked() {
  const paymentDataRequest = getGooglePaymentDataRequest();
  paymentDataRequest.transactionInfo = getGoogleTransactionInfo();

  const paymentsClient = getGooglePaymentsClient();
  paymentsClient
    .loadPaymentData(paymentDataRequest)
    .then(function (paymentData) {
      // handle the response
      processPayment(paymentData);
    })
    .catch(function (err) {
      // show error in developer console for debugging
      console.error(err);
    });
}

function processPayment(paymentData) {
  // show returned data in developer console for debugging
  console.log("processPayment", paymentData);
  // @todo pass payment token to your gateway to process payment
  paymentToken = paymentData.paymentMethodData.tokenizationData.token;
  console.log("TOKEN", paymentToken);
}

onGooglePayLoaded();
