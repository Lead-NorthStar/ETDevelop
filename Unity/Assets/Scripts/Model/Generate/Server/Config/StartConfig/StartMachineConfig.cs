
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;

namespace ET
{
    [EnableClass]
    public sealed partial class StartMachineConfig : BeanBase
    {
        public StartMachineConfig(ByteBuf _buf)
        {
            Id = _buf.ReadInt();
            InnerIP = _buf.ReadString();
            OuterIP = _buf.ReadString();
            WatcherPort = _buf.ReadString();

            PostInit();
        }

        public static StartMachineConfig DeserializeStartMachineConfig(ByteBuf _buf)
        {
            return new StartMachineConfig(_buf);
        }

        /// <summary>
        /// 这是id
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// 内网地址
        /// </summary>
        public readonly string InnerIP;

        /// <summary>
        /// 外网地址
        /// </summary>
        public readonly string OuterIP;

        /// <summary>
        /// 守护进程端口
        /// </summary>
        public readonly string WatcherPort;


        public const int __ID__ = 1628109127;
        public override int GetTypeId() => __ID__;

        public override string ToString()
        {
            return "{ "
            + "Id:" + Id + ","
            + "InnerIP:" + InnerIP + ","
            + "OuterIP:" + OuterIP + ","
            + "WatcherPort:" + WatcherPort + ","
            + "}";
        }

        partial void PostInit();
    }
}
