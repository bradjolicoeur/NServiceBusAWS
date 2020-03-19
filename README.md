# NServiceBusAWS

This is an example project that demonstrates using NServiceBus with AWS SQS and MongoDB for persistance.

The demo is configured to use localstack and mongodb in docker containers.  Use the docker-compose.yml file above to start up the localstack and mongodb containers before running the project.  

### Workflow

1. Web API controller sends ProcessPayment Command to Saga Endpoint
2. ProcessPayment initializes Saga state in PaymentSaga endpoint and sets a reply timeout
3. ProcessPaymentSaga sends MakePayment Command to PaymentProcessorWorker Endpoint
4. MakePayment handler simulates making a payment and publishes ICompletedMakePayment event
5. ProcessPaymentSaga consumes ICompletedMakePayment, updates state and sends reply to controller
6. Controller displays the results to the caller



If step 4 is delayed, the timeout will triger sending a 'Pending' reply to the controller and the eventual accept message will still get handled in the web api assembly for sending an asyncronous message to the caller.

Note that any keys or passwords included in this repo are provided as examples only.  Make sure you update keys and passwords to appropriate values in your system.
