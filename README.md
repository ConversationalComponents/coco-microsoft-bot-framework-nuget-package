# CoCo .NET CORE SDK For Microsoft Bot Framework

SDK for implementing Conversational Components from CoCoHub in your Bot: 
[https://conversationalcomponents.com](https://conversationalcomponents.com)


## Installation:

```
dotnet add package CoCoMicrosoftBotFrameworkSDK --version 1.0.3
```

## Setup:

* In Setup.cs:


   - Add **ConversationState**:
   ```
    var storage = new MemoryStorage();

    var conversationState = new ConversationState(storage);

    services.AddSingleton(conversationState);
   ```
   
* At your Bot class:


   - Bot class has to be inherited for **CoCoActivityHandler**:
   ```
       public class ExampleBot : CoCoActiviyHandler
      {
          public ExampleBot(ConversationState conversationState): base(conversationState)
          {
              this.ConversationState = conversationState;
          }
       ...
   ```
   - At the **OnMessageActivityAsync** method set check for active component:
   ```
      if(IsComponentActive(turnContext))
      {
          CallActiveComponent(turnContext);
          return;
      }
   ```
   else, Implement bot answer. 
   ```
      var replyText = $"Echo: {turnContext.Activity.Text}";
      await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
   ```
   - At the **OnMembersAddedAsync** method activate CoCo Components.
   ```
      var welcomeText = "Hello and welcome!";
      foreach (var member in membersAdded)
      {
          if (member.Id != turnContext.Activity.Recipient.Id)
          {
              await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
              ActivateComponent(turnContext, "namer_vp3");
          }
      }
   ```
    
