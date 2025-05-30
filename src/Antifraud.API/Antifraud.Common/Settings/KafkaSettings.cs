﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antifraud.Common.Settings
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public string TransactionsTopic { get; set; }
        public string ConfirmationTopic { get; set; }
        public string AutoOffsetReset { get; set; }
    }
}
