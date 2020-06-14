using System;
using System.Collections.Generic;
using System.Linq;
using DataLoader.DB.Entities;

namespace DataLoader.Utils
{
    public class MessagesToCumulativeSum
    {
        public static Dictionary<DateTime, int> Convert(List<MessageEntity> messages)
        {
            var cumulativeSumDictionary = new Dictionary<DateTime, int>();

            var sum = 0;
            messages = messages.OrderBy(x => x.SendTime).ToList();
            
            if (messages.Count == 0)
            {
                return cumulativeSumDictionary;
            }

            var currentDay = messages[0].SendTime.Date;

            foreach (var message in messages)
            {
                if (currentDay == message.SendTime.Date)
                {
                    sum++;
                }
                else
                {
                    cumulativeSumDictionary.Add(currentDay, sum);
                    currentDay = currentDay.AddDays(1);
                }
            }

            return cumulativeSumDictionary;
        }
    }
}