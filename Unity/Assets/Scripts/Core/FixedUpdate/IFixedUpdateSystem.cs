using System;

namespace ET
{
	public interface IFixedUpdate
	{
	}
	
	public interface IFixedUpdate<A>
	{
	}
	
	public interface IFixedUpdateSystem: ISystemType
	{
		void Run(Entity o);
	}
	
	public interface IFixedUpdateSystem<A>: ISystemType
	{
		void Run(Entity o, A a);
	}

	[EntitySystem]
	public abstract class FixedUpdateSystem<T> : SystemObject, IFixedUpdateSystem where T: Entity, IFixedUpdate
	{
		Type ISystemType.Type()
		{
			return typeof(T);
		}

		Type ISystemType.SystemType()
		{
			return typeof(IFixedUpdateSystem);
		}

		int ISystemType.GetInstanceQueueIndex()
		{
			return InstanceQueueIndex.FixedUpdate;
		}
		
		void IFixedUpdateSystem.Run(Entity o)
		{
			this.FixedUpdate((T)o);
		}

		protected abstract void FixedUpdate(T self);
	}
	
	[EntitySystem]
	public abstract class FixedUpdateSystem<T, A> : SystemObject, IFixedUpdateSystem<A> where T: Entity, IFixedUpdate<A>
	{
		Type ISystemType.Type()
		{
			return typeof(T);
		}

		Type ISystemType.SystemType()
		{
			return typeof(IFixedUpdateSystem<A>);
		}

		int ISystemType.GetInstanceQueueIndex()
		{
			return InstanceQueueIndex.FixedUpdate;
		}
		
		void IFixedUpdateSystem<A>.Run(Entity o, A a)
		{
			this.FixedUpdate((T)o, a);
		}

		protected abstract void FixedUpdate(T self, A a);
	}
}
