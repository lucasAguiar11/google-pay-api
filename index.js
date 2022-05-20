const baseRequest = {
  apiVersion: 2,
  apiVersionMinor: 0,
};

// chaves publicas do google, possuem um campo keyExpiration que indica quando a chave expira no cache
// Teste: https://payments.developers.google.com/paymentmethodtoken/test/keys.json
// Produção: https://payments.developers.google.com/paymentmethodtoken/keys.json

// integração direta
// chave publica - renovar uma vez por ano

const tokenizationSpecification = {
  type: "DIRECT",
  parameters: {
    protocolVersion: "ECv2",
    publicKey:"BL/Lh06N8v8Duc0dypkRWBkXz+lSEjFA7M67G3AWhSpnHYOePlQJm+7jvmnqL1cwcQkN8Ke0NQGQyb4A4kuqNLM=",
  },
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

const baseCardPaymentMethod = {
  type: "CARD",
  parameters: {
    allowedAuthMethods: allowedCardAuthMethods,
    allowedCardNetworks: allowedCardNetworks,
  },
};

const cardPaymentMethod = Object.assign(
  { tokenizationSpecification: tokenizationSpecification },
  baseCardPaymentMethod
);

const paymentsClient = new google.payments.api.PaymentsClient({
  environment: "TEST", // PRODUCTION
});

const isReadyToPayRequest = Object.assign({}, baseRequest);
isReadyToPayRequest.allowedPaymentMethods = [baseCardPaymentMethod];

const paymentDataRequest = Object.assign({}, baseRequest);
paymentDataRequest.allowedPaymentMethods = [cardPaymentMethod];
paymentDataRequest.transactionInfo = getGoogleTransactionInfo();
paymentDataRequest.callbackIntents = ["PAYMENT_AUTHORIZATION"];

//https://developers.google.com/pay/api/web/reference/request-objects#TransactionInfo
paymentDataRequest.transactionInfo = {
  totalPriceStatus: "FINAL",
  totalPrice: "1.00",
  currencyCode: "BRL",
  countryCode: "BR",
};

paymentDataRequest.merchantInfo = {
  merchantName: "CELER TESTE",
  merchantId: "BCR2DN4TWDE2BSRJ",
};

const paymentOptions = {
  environment: "TEST",
  merchantInfo: paymentDataRequest.merchantInfo,
  paymentDataCallbacks: {
    onPaymentAuthorized: onPaymentAuthorized,
  },
};

//Verifica se o navegador esta pronto para usar o google pay
const isOkToPay = async () => {
  try {
    const response = await paymentsClient.isReadyToPay(isReadyToPayRequest);
    console.log(response);
    return response.result;
  } catch (error) {
    console.error(error);
    return false;
  }
};

const handlePayment = async () => {
  try {
    const paymentData = await paymentsClient.loadPaymentData(
      paymentDataRequest
    );
    const paymentToken = paymentData.paymentMethodData.tokenizationData.token;
    console.log("paymentData", paymentData);
    console.log("paymentData JSON", JSON.stringify(paymentData));

    console.log("token", paymentToken);
  } catch (error) {
    console.error(error);
  }
};

const onPaymentAuthorized = (paymentData) => {
  return new Promise(function (resolve, reject) {

    console.log("[CALL] onPaymentAuthorized", paymentData);

    // handle the response
    processPayment(paymentData)
      .then(function () {
        resolve({ transactionState: "SUCCESS" });
      })
      .catch(function () {
        resolve({
          transactionState: "ERROR",
          error: {
            intent: "PAYMENT_AUTHORIZATION",
            message: "Insufficient funds",
            reason: "PAYMENT_DATA_INVALID",
          },
        });
      });
  });
};

// pre-busca dos dados de pagamento
const handlePreData = () => {
  const preData = paymentsClient.prefetchPaymentData(paymentDataRequest);
  console.log("handlePreData", preData);
};

//add btn para pagamento
const button = paymentsClient.createButton({
  onClick: async () => {
    await handlePayment();
  },
  allowedPaymentMethods: [],
}); // make sure to provide an allowed payment method

isOkToPay()
  .then((result) => {
    if (!result) return;

    handlePreData();

    document.getElementById("div-container-btn").appendChild(button);
  })
  .catch((error) => {
    console.log(error);
  });
