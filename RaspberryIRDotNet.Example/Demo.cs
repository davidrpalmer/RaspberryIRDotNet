using System;

namespace RaspberryIRDotNetExamples
{
    abstract class Demo
    {
        public abstract string Name { get; }

        public abstract void Run();

        protected void RequireTx()
        {
            if (string.IsNullOrEmpty(DemoConfig.GetTxDevice()))
            {
                throw new Exception("You must specify a TX device in DemoConfig.cs to use this demo.");
            }
        }

        protected void RequireRx()
        {
            if (string.IsNullOrEmpty(DemoConfig.GetRxDevice()))
            {
                throw new Exception("You must specify a RX device in DemoConfig.cs to use this demo.");
            }
        }
    }
}
