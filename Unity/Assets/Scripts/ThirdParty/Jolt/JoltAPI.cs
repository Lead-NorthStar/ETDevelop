﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Jolt
{
    internal static unsafe class JoltAPI
    {
        private static NativeHandlePool handles = new (1024);

        private static Dictionary<IntPtr, IContactListener> managedContactListeners = new (); // TODO use unmanaged container for Burst compatability

        private static Dictionary<IntPtr, IBodyActivationListener> managedBodyActivationListeners = new (); // TODO use unmanaged container for Burst compatability

        #region Handle Management

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeHandle<T> CreateHandle<T>(T* ptr) where T : unmanaged
        {
            return handles.CreateHandle(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeOwnedHandle<U> CreateOwnedHandle<T, U>(NativeHandle<T> owner, U* ptr) where T : unmanaged where U : unmanaged
        {
            return handles.CreateOwnedHandle(owner, ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DisposeHandle<T>(NativeHandle<T> handle) where T : unmanaged
        {
            handles.DisposeHandle(handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T* GetPointer<T>(NativeHandle<T> handle) where T : unmanaged
        {
            return handles.GetPointer(handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T* GetOwnedPointer<T>(NativeOwnedHandle<T> handle) where T : unmanaged
        {
            return handles.GetOwnedPointer(handle);
        }

        #endregion

        static JoltAPI()
        {
            // Set global static contact and body activation listeners. These are invoked by joltc
            // with the listener, which we use as a key to find the associated managed callbacks.

            Bindings.JPH_ContactListener_SetProcs(new JPH_ContactListener_Procs
            {
                OnContactValidate  = Marshal.GetFunctionPointerForDelegate(OnContactValidateDelegate),
                OnContactAdded     = Marshal.GetFunctionPointerForDelegate(OnContactAddedDelegate),
                OnContactPersisted = Marshal.GetFunctionPointerForDelegate(OnContactPersistedDelegate),
                OnContactRemoved   = Marshal.GetFunctionPointerForDelegate(OnContactRemovedDelegate),
            });

            Bindings.JPH_BodyActivationListener_SetProcs(new JPH_BodyActivationListener_Procs
            {
                OnBodyActivated   = Marshal.GetFunctionPointerForDelegate(OnBodyActivatedDelegate),
                OnBodyDeactivated = Marshal.GetFunctionPointerForDelegate(OnBodyDeactivatedDelegate),
            });
        }

        #region JPH

        public static bool JPH_Init(uint tempAllocatorSize)
        {
            return Bindings.JPH_Init(tempAllocatorSize);
        }

        public static void JPH_Shutdown()
        {
            Bindings.JPH_Shutdown();
        }

        public static void JPH_SetAssertFailureHandler()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region JPH_BroadPhaseLayerInterface

        public static NativeHandle<JPH_BroadPhaseLayerInterface> JPH_BroadPhaseLayerInterfaceMask_Create(uint numBroadPhaseLayers)
        {
            throw new NotImplementedException();
        }

        public static void JPH_BroadPhaseLayerInterfaceMask_ConfigureLayer(NativeHandle<JPH_BroadPhaseLayerInterface> @interface, byte broadPhaseLayer, uint groupsToInclude, uint groupsToExclude)
        {
            throw new NotImplementedException();
        }

        public static NativeHandle<JPH_BroadPhaseLayerInterface> JPH_BroadPhaseLayerInterfaceTable_Create(uint numObjectLayers, uint numBroadPhaseLayers)
        {
            throw new NotImplementedException();
        }

        public static void JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(NativeHandle<JPH_BroadPhaseLayerInterface> @interface, ushort objectLayer, byte broadPhaseLayer)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region JPH_ObjectLayerPairFilterMask

        public static NativeHandle<JPH_ObjectLayerPairFilter> JPH_ObjectLayerPairFilterMask_Create()
        {
            throw new NotImplementedException();
        }

        public static void JPH_ObjectLayerPairFilterMask_GetObjectLayer(uint group, uint mask)
        {
            Bindings.JPH_ObjectLayerPairFilterMask_GetObjectLayer(group, mask); // TODO no handle?
        }

        public static void JPH_ObjectLayerPairFilterMask_GetGroup(ushort layer)
        {
            Bindings.JPH_ObjectLayerPairFilterMask_GetGroup(layer); // TODO no handle?
        }

        public static void JPH_ObjectLayerPairFilterMask_GetMask(ushort layer)
        {
            Bindings.JPH_ObjectLayerPairFilterMask_GetMask(layer); // TODO no handle?
        }

        #endregion

        #region JPH_ObjectLayerPairFilterTable

        public static NativeHandle<JPH_ObjectLayerPairFilter> JPH_ObjectLayerPairFilterTable_Create(uint numObjectLayers)
        {
            return CreateHandle(Bindings.JPH_ObjectLayerPairFilterTable_Create(numObjectLayers));
        }

        public static void JPH_ObjectLayerPairFilterTable_DisableCollision(NativeHandle<JPH_ObjectLayerPairFilter> filter, ushort layerA, ushort layerB)
        {
            Bindings.JPH_ObjectLayerPairFilterTable_DisableCollision(GetPointer(filter), layerA, layerB);
        }

        public static void JPH_ObjectLayerPairFilterTable_EnableCollision(NativeHandle<JPH_ObjectLayerPairFilter> filter, ushort layerA, ushort layerB)
        {
            Bindings.JPH_ObjectLayerPairFilterTable_EnableCollision(GetPointer(filter), layerA, layerB);
        }

        #endregion

        #region JPH_ObjectVsBroadPhaseLayerFilterMask

        public static NativeHandle<JPH_ObjectVsBroadPhaseLayerFilter> JPH_ObjectVsBroadPhaseLayerFilterMask_Create(NativeHandle<JPH_BroadPhaseLayerInterface> broadPhaseLayerInterface)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region JPH_ObjectVsBroadPhaseLayerFilterTable

        public static NativeHandle<JPH_ObjectVsBroadPhaseLayerFilter> JPH_ObjectVsBroadPhaseLayerFilterTable_Create(NativeHandle<JPH_BroadPhaseLayerInterface> broadPhaseLayerInterface, uint numBroadPhaseLayers, NativeHandle<JPH_ObjectLayerPairFilter> objectLayerPairFilter, uint numObjectLayers)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region JPH_ContactListener

        private static void OnContactValidateCallback(JPH_ContactListener* listener, JPH_Body* bodyA, JPH_Body* bodyB, double3* offset, JPH_CollideShapeResult* result)
        {
            if (managedContactListeners.TryGetValue((IntPtr) listener, out var value))
            {
                value.OnContactValidate(); // TODO add args
            }
        }

        private static void OnContactAddedCallback(JPH_ContactListener* listener, JPH_Body* bodyA, JPH_Body* bodyB)
        {
            if (managedContactListeners.TryGetValue((IntPtr) listener, out var value))
            {
                value.OnContactAdded(); // TODO add args
            }
        }

        private static void OnContactPersistedCallback(JPH_ContactListener* listener, JPH_Body* bodyA, JPH_Body* bodyB)
        {
            if (managedContactListeners.TryGetValue((IntPtr) listener, out var value))
            {
                value.OnContactPersisted(); // TODO add args
            }
        }

        private static void OnContactRemovedCallback(JPH_ContactListener* listener, JPH_SubShapeIDPair* pair)
        {
            if (managedContactListeners.TryGetValue((IntPtr) listener, out var value))
            {
                value.OnContactRemoved(); // TODO add args
            }
        }

        // Define static delegates so we can marshal function pointers.

        private delegate void OnContactValidate(JPH_ContactListener* listener, JPH_Body* bodyA, JPH_Body* bodyB, double3* offset, JPH_CollideShapeResult* result);

        private static readonly OnContactValidate OnContactValidateDelegate = OnContactValidateCallback;

        private delegate void OnContactAdded(JPH_ContactListener* listener, JPH_Body* bodyA, JPH_Body* bodyB);

        private static readonly OnContactAdded OnContactAddedDelegate = OnContactAddedCallback;

        private delegate void OnContactPersisted(JPH_ContactListener* listener, JPH_Body* bodyA, JPH_Body* bodyB);

        private static readonly OnContactPersisted OnContactPersistedDelegate = OnContactPersistedCallback;

        private delegate void OnContactRemoved(JPH_ContactListener* listener, JPH_SubShapeIDPair* pair);

        private static readonly OnContactRemoved OnContactRemovedDelegate = OnContactRemovedCallback;

        #endregion

        #region JPH_BodyActivationListener

        private static void OnBodyActivatedCallback(JPH_BodyActivationListener* listener, BodyID bodyID, ulong bodyUserData)
        {
            if (managedBodyActivationListeners.TryGetValue((IntPtr) listener, out var value))
            {
                value.OnBodyActivated(bodyID, bodyUserData);
            }
        }

        private static void OnBodyDeactivatedCallback(JPH_BodyActivationListener* listener, BodyID bodyID, ulong bodyUserData)
        {
            if (managedBodyActivationListeners.TryGetValue((IntPtr)listener, out var value))
            {
                value.OnBodyDeactivated(bodyID, bodyUserData);
            }
        }

        private delegate void OnBodyActivated(JPH_BodyActivationListener* listener, BodyID bodyID, ulong bodyUserData);

        private static readonly OnBodyActivated OnBodyActivatedDelegate = OnBodyActivatedCallback;

        private delegate void OnBodyDeactivated(JPH_BodyActivationListener* listener, BodyID bodyID, ulong bodyUserData);

        private static readonly OnBodyDeactivated OnBodyDeactivatedDelegate = OnBodyDeactivatedCallback;

        #endregion

        #region JPH_PhysicsSystem

        public static NativeHandle<JPH_PhysicsSystem> JPH_PhysicsSystem_Create(PhysicsSystemSettings settings, out NativeOwnedHandle<JPH_ObjectLayerPairFilter> h1, out NativeOwnedHandle<JPH_BroadPhaseLayerInterface> h2, out NativeOwnedHandle<JPH_ObjectVsBroadPhaseLayerFilter> h3)
        {
            JPH_PhysicsSystemSettings nativeSettings = default;

            nativeSettings.maxBodies = settings.MaxBodies;
            nativeSettings.maxBodyPairs = settings.MaxBodyPairs;
            nativeSettings.maxContactConstraints = settings.MaxContactConstraints;

            // TODO take these as args

            nativeSettings.objectLayerPairFilter = Bindings.JPH_ObjectLayerPairFilterTable_Create(2);

            Bindings.JPH_ObjectLayerPairFilterTable_EnableCollision(nativeSettings.objectLayerPairFilter, 0, 1);
            Bindings.JPH_ObjectLayerPairFilterTable_EnableCollision(nativeSettings.objectLayerPairFilter, 1, 1);

            nativeSettings.broadPhaseLayerInterface = Bindings.JPH_BroadPhaseLayerInterfaceMask_Create(2);

            Bindings.JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(nativeSettings.broadPhaseLayerInterface, 0, 0);
            Bindings.JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(nativeSettings.broadPhaseLayerInterface, 1, 1);

            nativeSettings.objectVsBroadPhaseLayerFilter = Bindings.JPH_ObjectVsBroadPhaseLayerFilterTable_Create(nativeSettings.broadPhaseLayerInterface, 2, nativeSettings.objectLayerPairFilter, 2);

            var system = CreateHandle(Bindings.JPH_PhysicsSystem_Create(&nativeSettings));

            h1 = CreateOwnedHandle(system, nativeSettings.objectLayerPairFilter);
            h2 = CreateOwnedHandle(system, nativeSettings.broadPhaseLayerInterface);
            h3 = CreateOwnedHandle(system, nativeSettings.objectVsBroadPhaseLayerFilter);

            return system;
        }

        public static void JPH_PhysicsSystem_Destroy(NativeHandle<JPH_PhysicsSystem> handle)
        {
            Bindings.JPH_PhysicsSystem_Destroy(GetPointer(handle));
            DisposeHandle(handle);
        }

        public static void JPH_PhysicsSystem_OptimizeBroadPhase(NativeHandle<JPH_PhysicsSystem> handle)
        {
            Bindings.JPH_PhysicsSystem_OptimizeBroadPhase(GetPointer(handle));
        }

        public static PhysicsUpdateError JPH_PhysicsSystem_Step(NativeHandle<JPH_PhysicsSystem> system, float deltaTime, int collisionSteps)
        {
            return Bindings.JPH_PhysicsSystem_Step(GetPointer(system), deltaTime, collisionSteps);
        }

        public static NativeOwnedHandle<JPH_BodyInterface> JPH_PhysicsSystem_GetBodyInterface(NativeHandle<JPH_PhysicsSystem> system)
        {
            return CreateOwnedHandle(system, Bindings.JPH_PhysicsSystem_GetBodyInterface(GetPointer(system)));
        }

        public static NativeOwnedHandle<JPH_BodyInterface> JPH_PhysicsSystem_GetBodyInterfaceNoLock(NativeHandle<JPH_PhysicsSystem> system)
        {
            return CreateOwnedHandle(system, Bindings.JPH_PhysicsSystem_GetBodyInterfaceNoLock(GetPointer(system)));
        }

        public static NativeOwnedHandle<JPH_BodyLockInterface> JPC_PhysicsSystem_GetBodyLockInterface(NativeHandle<JPH_PhysicsSystem> system)
        {
            return CreateOwnedHandle(system, Bindings.JPC_PhysicsSystem_GetBodyLockInterface(GetPointer(system)));
        }

        public static NativeOwnedHandle<JPH_BodyLockInterface> JPC_PhysicsSystem_GetBodyLockInterfaceNoLock(NativeHandle<JPH_PhysicsSystem> system)
        {
            return CreateOwnedHandle(system, Bindings.JPC_PhysicsSystem_GetBodyLockInterfaceNoLock(GetPointer(system)));
        }

        public static NativeOwnedHandle<JPH_NarrowPhaseQuery> JPC_PhysicsSystem_GetNarrowPhaseQuery(NativeHandle<JPH_PhysicsSystem> system)
        {
            return CreateOwnedHandle(system, Bindings.JPC_PhysicsSystem_GetNarrowPhaseQuery(GetPointer(system)));
        }

        public static NativeOwnedHandle<JPH_NarrowPhaseQuery> JPC_PhysicsSystem_GetNarrowPhaseQueryNoLock(NativeHandle<JPH_PhysicsSystem> system)
        {
            return CreateOwnedHandle(system, Bindings.JPC_PhysicsSystem_GetNarrowPhaseQueryNoLock(GetPointer(system)));
        }

        public static void JPH_PhysicsSystem_SetContactListener(NativeHandle<JPH_PhysicsSystem> system, IContactListener listener)
        {
            var nativeContactListener = Bindings.JPH_ContactListener_Create();

            Bindings.JPH_PhysicsSystem_SetContactListener(GetPointer(system), nativeContactListener);

            managedContactListeners.Add((IntPtr) nativeContactListener, listener);
        }

        public static void JPH_PhysicsSystem_SetBodyActivationListener(NativeHandle<JPH_PhysicsSystem> system, IBodyActivationListener listener)
        {
            var nativeBodyActivationListener = Bindings.JPH_BodyActivationListener_Create();

            Bindings.JPH_PhysicsSystem_SetBodyActivationListener(GetPointer(system), nativeBodyActivationListener);

            managedBodyActivationListeners.Add((IntPtr) nativeBodyActivationListener, listener);
        }

        public static uint JPH_PhysicsSystem_GetNumBodies(NativeHandle<JPH_PhysicsSystem> system)
        {
            return Bindings.JPH_PhysicsSystem_GetNumBodies(GetPointer(system));
        }

        public static uint JPH_PhysicsSystem_GetNumActiveBodies(NativeHandle<JPH_PhysicsSystem> system, BodyType type)
        {
            return Bindings.JPH_PhysicsSystem_GetNumActiveBodies(GetPointer(system), type);
        }

        public static uint JPH_PhysicsSystem_GetMaxBodies(NativeHandle<JPH_PhysicsSystem> system)
        {
            return Bindings.JPH_PhysicsSystem_GetMaxBodies(GetPointer(system));
        }

        public static void JPH_PhysicsSystem_SetGravity(NativeHandle<JPH_PhysicsSystem> system, float3 gravity)
        {
            Bindings.JPH_PhysicsSystem_SetGravity(GetPointer(system), &gravity);
        }

        public static float3 JPH_PhysicsSystem_GetGravity(NativeHandle<JPH_PhysicsSystem> system)
        {
            float3 gravity = default;

            Bindings.JPH_PhysicsSystem_GetGravity(GetPointer(system), &gravity);

            return gravity;
        }

        public static void JPH_PhysicsSystem_AddConstraint(NativeHandle<JPH_PhysicsSystem> system)
        {
            throw new NotImplementedException();
        }

        public static void JPH_PhysicsSystem_RemoveConstraint(NativeHandle<JPH_PhysicsSystem> system)
        {
            throw new NotImplementedException();
        }

        public static void JPH_PhysicsSystem_AddConstraints(NativeHandle<JPH_PhysicsSystem> system)
        {
            throw new NotImplementedException();
        }

        public static void JPH_PhysicsSystem_RemoveConstraints(NativeHandle<JPH_PhysicsSystem> system)
        {
            throw new NotImplementedException();
        }

        #endregion

        // JPH_Quaternion_FromTo covered by Unity.Mathematics

        #region JPH_ShapeSettings

        public static void JPH_ShapeSettings_Destroy<T>(NativeHandle<T> settings) where T : unmanaged, INativeShapeSettings
        {
            Bindings.JPH_ShapeSettings_Destroy((JPH_ShapeSettings*) GetPointer(settings));
            DisposeHandle(settings);
        }

        #endregion

        #region JPH_ConvexShape

        public static float JPH_ConvexShape_GetDensity<T>(NativeHandle<T> shape) where T : unmanaged, INativeConvexShape
        {
            return Bindings.JPH_ConvexShape_GetDensity((JPH_ConvexShape*) GetPointer(shape));
        }

        public static void JPH_ConvexShape_SetDensity<T>(NativeHandle<T> shape, float density) where T : unmanaged, INativeConvexShape
        {
            Bindings.JPH_ConvexShape_SetDensity((JPH_ConvexShape*) GetPointer(shape), density);
        }

        #endregion

        #region JPH_BoxShapeSettings

        public static NativeHandle<JPH_BoxShapeSettings> JPH_BoxShapeSettings_Create(float3 halfExtent, float convexRadius)
        {
            return CreateHandle(Bindings.JPH_BoxShapeSettings_Create(&halfExtent, convexRadius));
        }

        public static NativeHandle<JPH_BoxShape> JPH_BoxShapeSettings_CreateShape(NativeHandle<JPH_BoxShapeSettings> settings)
        {
            return CreateHandle(Bindings.JPH_BoxShapeSettings_CreateShape(GetPointer(settings)));
        }

        #endregion

        #region JPH_BoxShape

        public static NativeHandle<JPH_BoxShape> JPH_BoxShape_Create(float3 halfExtent, float convexRadius)
        {
            return CreateHandle(Bindings.JPH_BoxShape_Create(&halfExtent, convexRadius));
        }

        public static float3 JPH_BoxShape_GetHalfExtent(NativeHandle<JPH_BoxShape> shape)
        {
            float3 result = default;

            Bindings.JPH_BoxShape_GetHalfExtent(GetPointer(shape), &result);

            return result;
        }

        public static float JPH_BoxShape_GetVolume(NativeHandle<JPH_BoxShape> shape)
        {
            return Bindings.JPH_BoxShape_GetVolume(GetPointer(shape));
        }

        public static float JPH_BoxShape_GetConvexRadius(NativeHandle<JPH_BoxShape> shape)
        {
            return Bindings.JPH_BoxShape_GetConvexRadius(GetPointer(shape));
        }

        #endregion

        #region JPH_SphereShapeSettings

        public static NativeHandle<JPH_SphereShapeSettings> JPH_SphereShapeSettings_Create(float radius)
        {
            return CreateHandle(Bindings.JPH_SphereShapeSettings_Create(radius));
        }

        public static NativeHandle<JPH_SphereShape> JPH_SphereShapeSettings_CreateShape(NativeHandle<JPH_SphereShapeSettings> settings)
        {
            return CreateHandle(Bindings.JPH_SphereShapeSettings_CreateShape(GetPointer(settings)));
        }

        public static float JPH_SphereShapeSettings_GetRadius(NativeHandle<JPH_SphereShapeSettings> settings)
        {
            return Bindings.JPH_SphereShapeSettings_GetRadius(GetPointer(settings));
        }

        public static void JPH_SphereShapeSettings_SetRadius(NativeHandle<JPH_SphereShapeSettings> settings, float radius)
        {
            Bindings.JPH_SphereShapeSettings_SetRadius(GetPointer(settings), radius);
        }

        #endregion

        #region JPH_SphereShape

        public static NativeHandle<JPH_SphereShape> JPH_SphereShape_Create(float radius)
        {
            return CreateHandle(Bindings.JPH_SphereShape_Create(radius));
        }

        public static float JPH_SphereShape_GetRadius(NativeHandle<JPH_SphereShape> shape)
        {
            return Bindings.JPH_SphereShape_GetRadius(GetPointer(shape));
        }

        #endregion

        #region JPH_TriangleShapeSettings

        public static NativeHandle<JPH_TriangleShapeSettings> JPH_TriangleShapeSettings_Create(float3 va, float3 vb, float3 vc, float convexRadius)
        {
            return CreateHandle(Bindings.JPH_TriangleShapeSettings_Create(&va, &vb, &vc, convexRadius));
        }

        #endregion

        #region JPH_CapsuleShapeSettings

        public static NativeHandle<JPH_CapsuleShapeSettings> JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius)
        {
            return CreateHandle(Bindings.JPH_CapsuleShapeSettings_Create(halfHeightOfCylinder, radius));
        }

        #endregion

        #region JPH_CapsuleShape_Create

        public static NativeHandle<JPH_CapsuleShape> JPH_CapsuleShape_Create(float halfHeightOfCylinder, float radius)
        {
            return CreateHandle(Bindings.JPH_CapsuleShape_Create(halfHeightOfCylinder, radius));
        }

        public static float JPH_CapsuleShape_GetRadius(NativeHandle<JPH_CapsuleShape> shape)
        {
            return Bindings.JPH_CapsuleShape_GetRadius(GetPointer(shape));
        }

        public static float JPH_CapsuleShape_GetHalfHeightOfCylinder(NativeHandle<JPH_CapsuleShape> shape)
        {
            return Bindings.JPH_CapsuleShape_GetHalfHeightOfCylinder(GetPointer(shape));
        }

        #endregion

        #region JPH_CylinderShapeSettings

        public static NativeHandle<JPH_CylinderShapeSettings> JPH_CylinderShapeSettings_Create(float halfHeight, float radius, float convexRadius)
        {
            return CreateHandle(Bindings.JPH_CylinderShapeSettings_Create(halfHeight, radius, convexRadius));
        }

        #endregion

        #region JPH_CylinderShape

        public static NativeHandle<JPH_CylinderShape> JPH_CylinderShape_Create(float halfHeight, float radius)
        {
            return CreateHandle(Bindings.JPH_CylinderShape_Create(halfHeight, radius));
        }

        public static float JPH_CylinderShape_GetRadius(NativeHandle<JPH_CylinderShape> shape)
        {
            return Bindings.JPH_CylinderShape_GetRadius(GetPointer(shape));
        }

        public static float JPH_CylinderShape_GetHalfHeight(NativeHandle<JPH_CylinderShape> shape)
        {
            return Bindings.JPH_CylinderShape_GetHalfHeight(GetPointer(shape));
        }

        #endregion

        #region JPH_ConvexShapeSettings

        public static float JPH_ConvexShapeSettings_GetDensity<T>(NativeHandle<T> settings) where T : unmanaged, INativeConvexShapeSettings
        {
            return Bindings.JPH_ConvexShapeSettings_GetDensity((JPH_ConvexShapeSettings*) GetPointer(settings));
        }

        public static void JPH_ConvexShapeSettings_SetDensity<T>(NativeHandle<T> settings, float density) where T : unmanaged, INativeConvexShapeSettings
        {
            Bindings.JPH_ConvexShapeSettings_SetDensity((JPH_ConvexShapeSettings*) GetPointer(settings), density);
        }

        #endregion

        #region JPH_ConvexHullShapeSettings

        public static NativeHandle<JPH_ConvexHullShapeSettings> JPH_ConvexHullShapeSettings_Create(ReadOnlySpan<float3> points, float maxConvexRadius)
        {
            fixed (float3* pointsPtr = points)
            {
                return CreateHandle(Bindings.JPH_ConvexHullShapeSettings_Create(pointsPtr, (uint) points.Length, maxConvexRadius));
            }
        }

        public static NativeHandle<JPH_ConvexHullShape> JPH_ConvexHullShapeSettings_CreateShape(NativeHandle<JPH_ConvexHullShapeSettings> settings)
        {
            return CreateHandle(Bindings.JPH_ConvexHullShapeSettings_CreateShape(GetPointer(settings)));
        }

        #endregion

        #region JPH_MeshShapeSettings

        public static NativeHandle<JPH_MeshShapeSettings> JPH_MeshShapeSettings_Create(ReadOnlySpan<Triangle> triangles)
        {
            fixed (Triangle* trianglesPtr = triangles)
            {
                return CreateHandle(Bindings.JPH_MeshShapeSettings_Create(trianglesPtr, (uint) triangles.Length));
            }
        }

        public static NativeHandle<JPH_MeshShapeSettings> JPH_MeshShapeSettings_Create2(ReadOnlySpan<float3> vertices, ReadOnlySpan<IndexedTriangle> triangles)
        {
            fixed (float3* verticesPtr = vertices)
            fixed (IndexedTriangle* trianglesPtr = triangles)
            {
                return CreateHandle(Bindings.JPH_MeshShapeSettings_Create2(verticesPtr, (uint) vertices.Length, trianglesPtr, (uint) triangles.Length));
            }
        }

        public static void JPH_MeshShapeSettings_Sanitize(NativeHandle<JPH_MeshShapeSettings> settings)
        {
            Bindings.JPH_MeshShapeSettings_Sanitize(GetPointer(settings));
        }

        public static NativeHandle<JPH_MeshShape> JPH_MeshShapeSettings_CreateShape(NativeHandle<JPH_MeshShapeSettings> settings)
        {
            return CreateHandle(Bindings.JPH_MeshShapeSettings_CreateShape(GetPointer(settings)));
        }

        public static NativeHandle<JPH_HeightFieldShapeSettings> JPH_HeightFieldShapeSettings_Create(ReadOnlySpan<float> samples, ReadOnlySpan<float3> offset, ReadOnlySpan<float3> scale)
        {
            fixed (float* samplesPtr = samples)
            fixed (float3* offsetPtr = offset)
            fixed (float3* scalePtr = scale)
            {
                return CreateHandle(Bindings.JPH_HeightFieldShapeSettings_Create(samplesPtr, offsetPtr, scalePtr, (uint) samples.Length));
            }
        }

        public static void JPH_MeshShapeSettings_DetermineMinAndMaxSample(NativeHandle<JPH_HeightFieldShapeSettings> settings, out float min, out float max, out float quantization)
        {
            // TODO rename JPH_HeightFieldShapeSettings_DetermineMinAndMaxSample ?

            fixed (float* minPtr = &min)
            fixed (float* maxPtr = &max)
            fixed (float* quantizationPtr = &quantization)
            {
                Bindings.JPH_MeshShapeSettings_DetermineMinAndMaxSample(GetPointer(settings), minPtr, maxPtr, quantizationPtr);
            }
        }

        public static uint JPH_MeshShapeSettings_CalculateBitsPerSampleForError(NativeHandle<JPH_HeightFieldShapeSettings> settings, float maxError)
        {
            // TODO rename JPH_HeightFieldShapeSettings_CalculateBitsPerSampleForError ?

            return Bindings.JPH_MeshShapeSettings_CalculateBitsPerSampleForError(GetPointer(settings), maxError);
        }

        #endregion

        #region JPH_TaperedCapsuleShapeSettings

        public static NativeHandle<JPH_TaperedCapsuleShapeSettings> JPH_TaperedCapsuleShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius)
        {
            return CreateHandle(Bindings.JPH_TaperedCapsuleShapeSettings_Create(halfHeightOfTaperedCylinder, topRadius, bottomRadius));
        }

        #endregion

        #region JPH_CompoundShapeSetting

        public static void JPH_CompoundShapeSettings_AddShape<T, U>(NativeHandle<T> settings, float3 position, quaternion rotation, NativeHandle<U> shape, uint userData) where T : unmanaged, INativeCompoundShapeSettings where U : unmanaged, INativeShapeSettings
        {
            Bindings.JPH_CompoundShapeSettings_AddShape((JPH_CompoundShapeSettings*) GetPointer(settings), &position, &rotation, (JPH_ShapeSettings*) GetPointer(shape), userData);
        }

        public static void JPH_CompoundShapeSettings_AddShape2<T, U>(NativeHandle<T> settings, float3 position, quaternion rotation, NativeHandle<U> shape, uint userData) where T : unmanaged, INativeCompoundShapeSettings where U : unmanaged, INativeShape
        {
            Bindings.JPH_CompoundShapeSettings_AddShape2((JPH_CompoundShapeSettings*) GetPointer(settings), &position, &rotation, (JPH_Shape*) GetPointer(shape), userData);
        }

        public static NativeHandle<JPH_StaticCompoundShapeSettings> JPH_StaticCompoundShapeSettings_Create()
        {
            return CreateHandle(Bindings.JPH_StaticCompoundShapeSettings_Create());
        }

        public static NativeHandle<JPH_MutableCompoundShapeSettings> JPH_MutableCompoundShapeSettings_Create()
        {
            return CreateHandle(Bindings.JPH_MutableCompoundShapeSettings_Create());
        }

        public static NativeHandle<JPH_MutableCompoundShape> JPH_MutableCompoundShape_Create(NativeHandle<JPH_MutableCompoundShapeSettings> settings)
        {
            return CreateHandle(Bindings.JPH_MutableCompoundShape_Create(GetPointer(settings)));
        }


        #endregion

        #region JPH_RotatedTranslatedShapeSettings

        public static NativeHandle<JPH_RotatedTranslatedShapeSettings> JPH_RotatedTranslatedShapeSettings_Create(float3 position, quaternion rotation, NativeHandle<JPH_ShapeSettings> settings)
        {
            return CreateHandle(Bindings.JPH_RotatedTranslatedShapeSettings_Create(&position, &rotation, GetPointer(settings)));
        }

        public static NativeHandle<JPH_RotatedTranslatedShapeSettings> JPH_RotatedTranslatedShapeSettings_Create2(float3 position, quaternion rotation, NativeHandle<JPH_Shape> shape)
        {
            return CreateHandle(Bindings.JPH_RotatedTranslatedShapeSettings_Create2(&position, &rotation, GetPointer(shape)));
        }

        public static NativeHandle<JPH_RotatedTranslatedShape> JPH_RotatedTranslatedShapeSettings_CreateShape(NativeHandle<JPH_RotatedTranslatedShapeSettings> settings)
        {
            return CreateHandle(Bindings.JPH_RotatedTranslatedShapeSettings_CreateShape(GetPointer(settings)));
        }

        #endregion

        #region JPH_RotatedTranslatedShape

        public static NativeHandle<JPH_RotatedTranslatedShape> JPH_RotatedTranslatedShape_Create(float3 position, quaternion rotation, NativeHandle<JPH_Shape> shape)
        {
            return CreateHandle(Bindings.JPH_RotatedTranslatedShape_Create(&position, &rotation, GetPointer(shape)));
        }

        #endregion

        #region JPH_Shape

        public static void JPH_Shape_Destroy<T>(NativeHandle<T> handle) where T : unmanaged, INativeShape
        {
            Bindings.JPH_Shape_Destroy((JPH_Shape*) GetPointer(handle));
            DisposeHandle(handle);
        }

        public static AABox JPH_Shape_GetLocalBounds<T>(NativeHandle<T> shape) where T : unmanaged, INativeShape
        {
            AABox result;

            Bindings.JPH_Shape_GetLocalBounds((JPH_Shape*) GetPointer(shape), &result);

            return result;
        }

        public static MassProperties JPH_Shape_GetMassProperties<T>(NativeHandle<T> shape) where T : unmanaged, INativeShape
        {
            MassProperties result;

            Bindings.JPH_Shape_GetMassProperties((JPH_Shape*) GetPointer(shape), &result);

            return result;
        }

        public static float3 JPH_Shape_GetCenterOfMass<T>(NativeHandle<T> shape) where T : unmanaged, INativeShape
        {
            float3 result = default;

            Bindings.JPH_Shape_GetCenterOfMass((JPH_Shape*) GetPointer(shape), &result);

            return result;
        }

        public static float JPH_Shape_GetInnerRadius<T>(NativeHandle<T> shape) where T : unmanaged, INativeShape
        {
            return Bindings.JPH_Shape_GetInnerRadius((JPH_Shape*) GetPointer(shape));
        }

        #endregion

        #region JPH_BodyCreationSettings

        public static NativeHandle<JPH_BodyCreationSettings> JPH_BodyCreationSettings_Create()
        {
            return CreateHandle(Bindings.JPH_BodyCreationSettings_Create());
        }

        public static NativeHandle<JPH_BodyCreationSettings> JPH_BodyCreationSettings_Create2<T>(NativeHandle<T> settings, double3 position, quaternion rotation, MotionType motion, ushort layer) where T : unmanaged, INativeShapeSettings
        {
            return CreateHandle(Bindings.JPH_BodyCreationSettings_Create2((JPH_ShapeSettings*) GetPointer(settings), &position, &rotation, motion, layer));
        }

        public static NativeHandle<JPH_BodyCreationSettings> JPH_BodyCreationSettings_Create3<T>(NativeHandle<T> shape, double3 position, quaternion rotation, MotionType motion, ushort layer) where T : unmanaged, INativeShape
        {
            return CreateHandle(Bindings.JPH_BodyCreationSettings_Create3((JPH_Shape*) GetPointer(shape), &position, &rotation, motion, layer));
        }

        public static void JPH_BodyCreationSettings_Destroy(NativeHandle<JPH_BodyCreationSettings> handle)
        {
            Bindings.JPH_BodyCreationSettings_Destroy(GetPointer(handle));

            DisposeHandle(handle);
        }

        public static float3 JPH_BodyCreationSettings_GetLinearVelocity(NativeHandle<JPH_BodyCreationSettings> settings)
        {
            float3 result;

            Bindings.JPH_BodyCreationSettings_GetLinearVelocity(GetPointer(settings), &result);

            return result;
        }

        public static void JPH_BodyCreationSettings_SetLinearVelocity(NativeHandle<JPH_BodyCreationSettings> settings, float3 velocity)
        {
            Bindings.JPH_BodyCreationSettings_SetLinearVelocity(GetPointer(settings), &velocity);
        }

        public static float3 JPH_BodyCreationSettings_GetAngularVelocity(NativeHandle<JPH_BodyCreationSettings> settings)
        {
            float3 result;

            Bindings.JPH_BodyCreationSettings_GetAngularVelocity(GetPointer(settings), &result);

            return result;
        }

        public static void JPH_BodyCreationSettings_SetAngularVelocity(NativeHandle<JPH_BodyCreationSettings> settings, float3 velocity)
        {
            Bindings.JPH_BodyCreationSettings_SetAngularVelocity(GetPointer(settings), &velocity);
        }

        public static MotionType JPH_BodyCreationSettings_GetMotionType(NativeHandle<JPH_BodyCreationSettings> settings)
        {
            return Bindings.JPH_BodyCreationSettings_GetMotionType(GetPointer(settings));
        }

        public static void JPH_BodyCreationSettings_SetMotionType(NativeHandle<JPH_BodyCreationSettings> settings, MotionType value)
        {
            Bindings.JPH_BodyCreationSettings_SetMotionType(GetPointer(settings), value);
        }

        public static AllowedDOFs JPH_BodyCreationSettings_GetAllowedDOFs(NativeHandle<JPH_BodyCreationSettings> settings)
        {
            return Bindings.JPH_BodyCreationSettings_GetAllowedDOFs(GetPointer(settings));
        }

        public static void JPH_BodyCreationSettings_SetAllowedDOFs(NativeHandle<JPH_BodyCreationSettings> settings, AllowedDOFs value)
        {
            Bindings.JPH_BodyCreationSettings_SetAllowedDOFs(GetPointer(settings), value);
        }

        #endregion

        #region JPH_SoftBodyCreationSettings

        public static NativeHandle<JPH_SoftBodyCreationSettings> JPH_SoftBodyCreationSettings()
        {
            return CreateHandle(Bindings.JPH_SoftBodyCreationSettings_Create());
        }

        #endregion

        #region JPH_SpringSettings

        // TODO

        #endregion

        #region JPH_ConstraintSettings

        // TODO

        #endregion

        #region JPH_Constraint

        // TODO

        #endregion

        #region JPH_DistanceConstraint

        // TODO

        #endregion

        #region JPH_PointConstraint

        // TODO

        #endregion

        #region JPH_HingConstraint

        // TODO

        #endregion

        #region JPH_SliderConstraint

        // TODO

        #endregion

        #region JPH_SwingTistConstraint

        // TODO

        #endregion

        #region JPH_SixDOFConstraint

        // TODO

        #endregion

        #region JPH_TwoBodyConstraint

        // TODO

        #endregion

        #region JPH_BodyInterface

        public static void JPH_BodyInterface_DestroyBody(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            Bindings.JPH_BodyInterface_DestroyBody(GetOwnedPointer(@interface), bodyID);

            // TODO mark any active body handles for this bodyID as disposed
        }

        public static BodyID JPH_BodyInterface_CreateAndAddBody(NativeOwnedHandle<JPH_BodyInterface> @interface, NativeHandle<JPH_BodyCreationSettings> settings, Activation activation)
        {
            return Bindings.JPH_BodyInterface_CreateAndAddBody(GetOwnedPointer(@interface), GetPointer(settings), activation);
        }

        public static NativeHandle<JPH_Body> JPH_BodyInterface_CreateBody(NativeOwnedHandle<JPH_BodyInterface> @interface, NativeHandle<JPH_BodyCreationSettings> settings)
        {
            return CreateHandle(Bindings.JPH_BodyInterface_CreateBody(GetOwnedPointer(@interface), GetPointer(settings)));
        }

        public static NativeHandle<JPH_Body> JPH_BodyInterface_CreateSoftBody(NativeOwnedHandle<JPH_BodyInterface> @interface, NativeHandle<JPH_SoftBodyCreationSettings> settings)
        {
            return CreateHandle(Bindings.JPH_BodyInterface_CreateSoftBody(GetOwnedPointer(@interface), GetPointer(settings)));
        }

        public static NativeHandle<JPH_Body> JPH_BodyInterface_CreateBodyWithID(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID, NativeHandle<JPH_BodyCreationSettings> settings)
        {
            return CreateHandle(Bindings.JPH_BodyInterface_CreateBodyWithID(GetOwnedPointer(@interface), bodyID, GetPointer(settings)));
        }

        public static NativeHandle<JPH_Body> JPH_BodyInterface_CreateBodyWithoutID(NativeOwnedHandle<JPH_BodyInterface> @interface, NativeHandle<JPH_BodyCreationSettings> settings)
        {
            return CreateHandle(Bindings.JPH_BodyInterface_CreateBodyWithoutID(GetOwnedPointer(@interface), GetPointer(settings)));
        }

        public static void JPH_BodyInterface_DestroyBodyWithoutID(NativeOwnedHandle<JPH_BodyInterface> @interface, NativeHandle<JPH_Body> body)
        {
            Bindings.JPH_BodyInterface_DestroyBodyWithoutID(GetOwnedPointer(@interface), GetPointer(body));
        }

        public static bool JPH_BodyInterface_AssignBodyID(NativeOwnedHandle<JPH_BodyInterface> @interface, NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_BodyInterface_AssignBodyID(GetOwnedPointer(@interface), GetPointer(body));
        }

        public static bool JPH_BodyInterface_AssignBodyID2(NativeOwnedHandle<JPH_BodyInterface> @interface, NativeHandle<JPH_Body> body, BodyID bodyID)
        {
            return Bindings.JPH_BodyInterface_AssignBodyID2(GetOwnedPointer(@interface), GetPointer(body), bodyID);
        }

        public static NativeHandle<JPH_Body> JPH_BodyInterface_UnassignBodyID(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            // TODO is CreateHandle correct? Does that create a duplicate pointer to the body?

            // return CreateHandle(Bindings.JPH_BodyInterface_UnassignBodyID(GetOwnedPointer(@interface), bodyID));

            throw new NotImplementedException();
        }

        public static void JPH_BodyInterface_AddBody(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID, Activation activation)
        {
            Bindings.JPH_BodyInterface_AddBody(GetOwnedPointer(@interface), bodyID, activation);
        }

        public static void JPH_BodyInterface_RemoveBody(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            Bindings.JPH_BodyInterface_RemoveBody(GetOwnedPointer(@interface), bodyID);
        }

        public static bool JPH_BodyInterface_IsActive(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            return Bindings.JPH_BodyInterface_IsActive(GetOwnedPointer(@interface), bodyID);
        }

        public static bool JPH_BodyInterface_IsAdded(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            return Bindings.JPH_BodyInterface_IsAdded(GetOwnedPointer(@interface), bodyID);
        }

        public static bool JPH_BodyInterface_GetBodyType(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            return Bindings.JPH_BodyInterface_IsAdded(GetOwnedPointer(@interface), bodyID);
        }

        public static void JPH_BodyInterface_SetLinearVelocity(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID, float3 velocity)
        {
            Bindings.JPH_BodyInterface_SetLinearVelocity(GetOwnedPointer(@interface), bodyID, &velocity);
        }

        public static float3 JPH_BodyInterface_GetLinearVelocity(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            float3 result;

            Bindings.JPH_BodyInterface_GetLinearVelocity(GetOwnedPointer(@interface), bodyID, &result);

            return result;
        }

        public static double3 JPH_BodyInterface_GetCenterOfMassPosition(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            double3 result;

            Bindings.JPH_BodyInterface_GetCenterOfMassPosition(GetOwnedPointer(@interface), bodyID, &result);

            return result;
        }

        public static MotionType JPH_BodyInterface_GetMotionType(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            return Bindings.JPH_BodyInterface_GetMotionType(GetOwnedPointer(@interface), bodyID);
        }

        public static void JPH_BodyInterface_SetMotionType(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID, MotionType motion, Activation activation)
        {
            Bindings.JPH_BodyInterface_SetMotionType(GetOwnedPointer(@interface), bodyID, motion, activation);
        }

        public static float JPH_BodyInterface_GetRestitution(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            return Bindings.JPH_BodyInterface_GetRestitution(GetOwnedPointer(@interface), bodyID);
        }

        public static void JPH_BodyInterface_SetRestitution(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID, float restitution)
        {
            Bindings.JPH_BodyInterface_SetRestitution(GetOwnedPointer(@interface), bodyID, restitution);
        }

        public static float JPH_BodyInterface_GetFriction(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            return Bindings.JPH_BodyInterface_GetFriction(GetOwnedPointer(@interface), bodyID);
        }

        public static void JPH_BodyInterface_SetFriction(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID, float friction)
        {
            Bindings.JPH_BodyInterface_SetFriction(GetOwnedPointer(@interface), bodyID, friction);
        }

        public static void JPH_BodyInterface_SetPosition(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID, double3 position, Activation activation)
        {
            Bindings.JPH_BodyInterface_SetPosition(GetOwnedPointer(@interface), bodyID, &position, activation);
        }

        public static double3 JPH_BodyInterface_GetPosition(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            double3 result;

            Bindings.JPH_BodyInterface_GetPosition(GetOwnedPointer(@interface), bodyID, &result);

            return result;
        }

        public static void JPH_BodyInterface_SetRotation(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID, quaternion rotation, Activation activation)
        {
            Bindings.JPH_BodyInterface_SetRotation(GetOwnedPointer(@interface), bodyID, &rotation, activation);
        }

        public static quaternion JPH_BodyInterface_GetRotation(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            quaternion result;

            Bindings.JPH_BodyInterface_GetRotation(GetOwnedPointer(@interface), bodyID, &result);

            return result;
        }

        // TODO

        public static rmatrix4x4 JPH_BodyInterface_GetWorldTransform(NativeOwnedHandle<JPH_BodyInterface> @interface, BodyID bodyID)
        {
            rmatrix4x4 result;

            Bindings.JPH_BodyInterface_GetWorldTransform(GetOwnedPointer(@interface), bodyID, &result);

            return result;
        }

        // TODO

        #endregion

        #region JPH_BodyLockInterface

        // TODO

        #endregion

        #region JPH_MotionProperties

        // TODO

        #endregion

        #region JPH_NarrowPhaseQuery

        // TODO

        #endregion

        #region JPH_ShapeCastSettings

        // TODO

        #endregion

        #region JPH_AllHit

        // TODO

        #endregion

        #region JPH_Body

        public static BodyID JPH_Body_GetID(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_GetID(GetPointer(body));
        }

        public static BodyType JPH_Body_GetBodyType(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_GetBodyType(GetPointer(body));
        }

        public static AABox JPH_Body_GetWorldSpaceBounds(NativeHandle<JPH_Body> body)
        {
            AABox result;

            Bindings.JPH_Body_GetWorldSpaceBounds(GetPointer(body), &result);

            return result;
        }

        public static float3 JPH_Body_GetWorldSpaceSurfaceNormal(NativeHandle<JPH_Body> body, uint subShapeID, double3 position)
        {
            float3 result;

            Bindings.JPH_Body_GetWorldSpaceSurfaceNormal(GetPointer(body), subShapeID, &position, &result);

            return result;
        }

        public static bool JPH_Body_IsActive(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_IsActive(GetPointer(body));
        }

        public static bool JPH_Body_IsStatic(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_IsStatic(GetPointer(body));
        }

        public static bool JPH_Body_IsKinematic(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_IsKinematic(GetPointer(body));
        }

        public static bool JPH_Body_IsDynamic(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_IsDynamic(GetPointer(body));
        }

        public static bool JPH_Body_IsSensor(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_IsSensor(GetPointer(body));
        }

        public static void JPH_Body_SetIsSensor(NativeHandle<JPH_Body> body, bool value)
        {
            Bindings.JPH_Body_SetIsSensor(GetPointer(body), value);
        }

        public static void JPH_Body_SetCollideKinematicVsNonDynamic(NativeHandle<JPH_Body> body, bool value)
        {
            Bindings.JPH_Body_SetCollideKinematicVsNonDynamic(GetPointer(body), value);
        }

        public static bool JPH_Body_GetCollideKinematicVsNonDynamic(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_GetCollideKinematicVsNonDynamic(GetPointer(body));
        }

        public static void JPH_Body_SetUseManifoldReduction(NativeHandle<JPH_Body> body, bool value)
        {
            Bindings.JPH_Body_SetUseManifoldReduction(GetPointer(body), value);
        }

        public static bool JPH_Body_GetUseManifoldReduction(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_GetUseManifoldReduction(GetPointer(body));
        }

        public static bool JPH_Body_GetUseManifoldReductionWithBody(NativeHandle<JPH_Body> body, NativeHandle<JPH_Body> other)
        {
            return Bindings.JPH_Body_GetUseManifoldReductionWithBody(GetPointer(body), GetPointer(body));
        }

        public static void JPH_Body_SetApplyGyroscopicForce(NativeHandle<JPH_Body> body, bool value)
        {
            Bindings.JPH_Body_SetApplyGyroscopicForce(GetPointer(body), value);
        }

        public static bool JPH_Body_GetApplyGyroscopicForce(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_GetApplyGyroscopicForce(GetPointer(body));
        }

        public static NativeOwnedHandle<JPH_MotionProperties> JPH_Body_GetMotionProperties(NativeHandle<JPH_Body> body)
        {
            return CreateOwnedHandle(body, Bindings.JPH_Body_GetMotionProperties(GetPointer(body)));
        }

        public static MotionType JPH_Body_GetMotionType(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_GetMotionType(GetPointer(body));
        }

        public static void JPH_Body_SetMotionType(NativeHandle<JPH_Body> body, MotionType motion)
        {
            Bindings.JPH_Body_SetMotionType(GetPointer(body), motion);
        }

        public static bool JPH_Body_GetAllowSleeping(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_GetAllowSleeping(GetPointer(body));
        }

        public static void JPH_Body_SetAllowSleeping(NativeHandle<JPH_Body> body, bool allowSleeping)
        {
            Bindings.JPH_Body_SetAllowSleeping(GetPointer(body), allowSleeping);
        }

        public static void JPH_Body_ResetSleepTimer(NativeHandle<JPH_Body> body)
        {
            Bindings.JPH_Body_ResetSleepTimer(GetPointer(body));
        }

        public static float JPH_Body_GetFriction(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_GetFriction(GetPointer(body));
        }

        public static void JPH_Body_SetFriction(NativeHandle<JPH_Body> body, float friction)
        {
            Bindings.JPH_Body_SetFriction(GetPointer(body), friction);
        }

        public static float JPH_Body_GetRestitution(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_GetRestitution(GetPointer(body));
        }

        public static void JPH_Body_SetRestitution(NativeHandle<JPH_Body> body, float restitution)
        {
            Bindings.JPH_Body_SetRestitution(GetPointer(body), restitution);
        }

        public static float3 JPH_Body_GetLinearVelocity(NativeHandle<JPH_Body> body)
        {
            float3 result;

            Bindings.JPH_Body_GetLinearVelocity(GetPointer(body), &result);

            return result;
        }

        public static void JPH_Body_SetLinearVelocity(NativeHandle<JPH_Body> body, float3 velocity)
        {
            Bindings.JPH_Body_SetLinearVelocity(GetPointer(body), &velocity);
        }

        public static float3 JPH_Body_GetAngularVelocity(NativeHandle<JPH_Body> body)
        {
            float3 result;

            Bindings.JPH_Body_GetAngularVelocity(GetPointer(body), &result);

            return result;
        }

        public static void JPH_Body_SetAngularVelocity(NativeHandle<JPH_Body> body, float3 velocity)
        {
            Bindings.JPH_Body_SetAngularVelocity(GetPointer(body), &velocity);
        }

        public static void JPH_Body_AddForce(NativeHandle<JPH_Body> body, float3 force)
        {
            Bindings.JPH_Body_AddForce(GetPointer(body), &force);
        }

        public static void JPH_Body_AddForceAtPosition(NativeHandle<JPH_Body> body, float3 force, double3 position)
        {
            Bindings.JPH_Body_AddForceAtPosition(GetPointer(body), &force, &position);
        }

        public static void JPH_Body_AddTorque(NativeHandle<JPH_Body> body, float3 force)
        {
            Bindings.JPH_Body_AddTorque(GetPointer(body), &force);
        }

        public static float3 JPH_Body_GetAccumulatedForce(NativeHandle<JPH_Body> body)
        {
            float3 result;

            Bindings.JPH_Body_GetAccumulatedForce(GetPointer(body), &result);

            return result;
        }

        public static float3 JPH_Body_GetAccumulatedTorque(NativeHandle<JPH_Body> body)
        {
            float3 result;

            Bindings.JPH_Body_GetAccumulatedTorque(GetPointer(body), &result);

            return result;
        }

        public static void JPH_Body_AddImpulse(NativeHandle<JPH_Body> body, float3 impulse)
        {
            Bindings.JPH_Body_AddImpulse(GetPointer(body), &impulse);
        }

        public static void JPH_Body_AddImpulseAtPosition(NativeHandle<JPH_Body> body, float3 impulse, double3 position)
        {
            Bindings.JPH_Body_AddImpulseAtPosition(GetPointer(body), &impulse, &position);
        }

        public static void JPH_Body_AddAngularImpulse(NativeHandle<JPH_Body> body, float3 angularImpulse)
        {
            Bindings.JPH_Body_AddAngularImpulse(GetPointer(body), &angularImpulse);
        }

        public static double3 JPH_Body_GetPosition(NativeHandle<JPH_Body> body)
        {
            double3 result;

            Bindings.JPH_Body_GetPosition(GetPointer(body), &result);

            return result;
        }

        public static quaternion JPH_Body_GetRotation(NativeHandle<JPH_Body> body)
        {
            quaternion result;

            Bindings.JPH_Body_GetRotation(GetPointer(body), &result);

            return result;
        }

        public static double3 JPH_Body_GetCenterOfMassPosition(NativeHandle<JPH_Body> body)
        {
            double3 result;

            Bindings.JPH_Body_GetCenterOfMassPosition(GetPointer(body), &result);

            return result;
        }

        public static rmatrix4x4 JPH_Body_GetWorldTransform(NativeHandle<JPH_Body> body)
        {
            rmatrix4x4 result;

            Bindings.JPH_Body_GetWorldTransform(GetPointer(body), &result);

            return result;
        }

        public static rmatrix4x4 JPH_Body_GetCenterOfMassTransform(NativeHandle<JPH_Body> body)
        {
            rmatrix4x4 result;

            Bindings.JPH_Body_GetCenterOfMassTransform(GetPointer(body), &result);

            return result;
        }

        public static void JPH_Body_SetUserData(NativeHandle<JPH_Body> body, ulong userData)
        {
            Bindings.JPH_Body_SetUserData(GetPointer(body), userData);
        }

        public static ulong JPH_Body_GetUserData(NativeHandle<JPH_Body> body)
        {
            return Bindings.JPH_Body_GetUserData(GetPointer(body));
        }

        #endregion
    }
}
