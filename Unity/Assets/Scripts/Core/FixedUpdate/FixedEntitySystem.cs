using System;
using System.Collections.Generic;

namespace ET
{
    public partial class EntitySystem
    {
        public void FixedUpdate<P1>(P1 p1)
        {
            Queue<EntityRef<Entity>> queue = this.queues[InstanceQueueIndex.FixedUpdate];
            int count = queue.Count;
            while (count-- > 0)
            {
                Entity component = queue.Dequeue();
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                switch (component)
                {
                    case IFixedUpdate:
                    {
                        List<SystemObject> iFixedUpdateSystems =
                                EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IFixedUpdateSystem));
                        if (iFixedUpdateSystems == null)
                        {
                            continue;
                        }

                        queue.Enqueue(component);

                        foreach (IFixedUpdateSystem iFixedUpdateSystem in iFixedUpdateSystems)
                        {
                            try
                            {
                                iFixedUpdateSystem.Run(component);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                            }
                        }

                        break;
                    }
                    case IFixedUpdate<P1>:
                    {
                        List<SystemObject> iFixedUpdateSystems =
                                EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IFixedUpdateSystem<P1>));
                        if (iFixedUpdateSystems == null)
                        {
                            continue;
                        }

                        queue.Enqueue(component);

                        foreach (IFixedUpdateSystem<P1> iFixedUpdateSystem in iFixedUpdateSystems)
                        {
                            try
                            {
                                iFixedUpdateSystem.Run(component, p1);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}