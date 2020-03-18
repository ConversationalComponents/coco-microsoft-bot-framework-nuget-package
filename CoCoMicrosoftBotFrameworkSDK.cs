using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using CoCoSDK;

namespace CoCoMicrosoftBotFrameworkSDK
{
    
    public class CoCoActiviyHandler: ActivityHandler 
    {
        public BotState ConversationState;
        public IStatePropertyAccessor<bool> cocoContext;
        public IStatePropertyAccessor<string> activeComponent;
        public IStatePropertyAccessor<string> sessionId;
        public IStatePropertyAccessor<Dictionary<string, string>> contextParams;

        public CoCo cocoHandler = new CoCo();

        public CoCoActiviyHandler(ConversationState ConversationState)
         {
            this.ConversationState = ConversationState;
            this.cocoContext = this.ConversationState.CreateProperty<bool>("cocoContext");
            this.activeComponent = this.ConversationState.CreateProperty<string>("activeComponent");
            this.sessionId = this.ConversationState.CreateProperty<string>("sessionId");
            this.contextParams = this.ConversationState.CreateProperty<Dictionary<string, string>>("contextParams");
        }
        public void ActivateComponent(ITurnContext turnContext, string componentId)
        {
            UpdateContext(turnContext, componentId);
            CallActiveComponent(turnContext);
        }

        public void UpdateContext(ITurnContext turnContext, string componentId) 
        {
            this.cocoContext.SetAsync(turnContext, true);
            this.sessionId.SetAsync(turnContext, turnContext.Activity.Conversation.Id);
            this.activeComponent.SetAsync(turnContext, componentId);
            this.ConversationState.SaveChangesAsync(turnContext);
        }

        public bool IsComponentActive(ITurnContext turnContext)
        {
            using(Task<bool> cocoContext = this.cocoContext.GetAsync(turnContext))
            {
                if(cocoContext.Result)
                {
                    return true;
                }
                return false;
            }
        }
        
        public async void CallActiveComponent(ITurnContext turnContext) 
        {
            using(Task<Dictionary<string, string>> contextParamsTask=this.contextParams.GetAsync(turnContext))
            using(Task<string> sessionIdTask=this.sessionId.GetAsync(turnContext))
            using(Task<string> componentIdTask=this.activeComponent.GetAsync(turnContext))
            {
                string componentId = componentIdTask.Result;
                string sessionId = sessionIdTask.Result;
                Dictionary<string, string> contextParams = contextParamsTask.Result;

                CoCoContext cocoResponse = this.cocoHandler.Exchange(componentId, sessionId, turnContext.Activity.Text, contextParams);

                await this.contextParams.SetAsync(turnContext, cocoResponse.updated_context);

                if(cocoResponse.component_done || cocoResponse.conmponent_failed)
                {
                    await this.cocoContext.SetAsync(turnContext, false);
                    await this.activeComponent.SetAsync(turnContext, null);
                    await this.sessionId.SetAsync(turnContext, null);
                }

                await this.ConversationState.SaveChangesAsync(turnContext);
                await turnContext.SendActivityAsync(cocoResponse.response);
            }
        }
    }
}
