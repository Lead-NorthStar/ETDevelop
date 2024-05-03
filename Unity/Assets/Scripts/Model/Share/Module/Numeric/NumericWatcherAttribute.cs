using System;

namespace ET
{
	[AttributeUsage(AttributeTargets.Class)]
	public class NumericWatcherAttribute : BaseAttribute
	{
		public SceneType SceneType { get; }
		
		public int NumericType { get; }

		public NumericWatcherAttribute(SceneType sceneType, NumericType type)
		{
			this.SceneType = sceneType;
			this.NumericType = (int)type;
		}
	}
}