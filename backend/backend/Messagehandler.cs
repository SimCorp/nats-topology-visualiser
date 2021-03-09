using System;
using System.Text;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace backend
{
    public class MessageHandler
    {


        public MessageHandler() {
            Console.WriteLine(this.GetType());
        }


        public static void IncommingMessageHandler<T>(object sender, MsgHandlerEventArgs e) where T : new(){
            
            Console.WriteLine(e.Message.ToString());

            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));
            
            JToken token = JObject.Parse(json.ToString());

            var data_json = token.SelectToken("data");
            //var server_json = token.SelectToken("server");

            T server = new T();

            try {
            server = JsonConvert.DeserializeObject<T>(data_json.ToString());
            } catch (Exception x) {
                Console.WriteLine(x.StackTrace);
            }

            //Console.WriteLine(server.server_name);
            //Console.WriteLine(server.in_msgs);
            //Console.WriteLine(string.Join("", server.connections));
        }

    }

}