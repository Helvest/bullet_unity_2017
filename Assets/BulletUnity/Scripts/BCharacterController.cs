﻿using BulletSharp;
using UnityEngine;

namespace BulletUnity
{
	[AddComponentMenu("Physics Bullet/Character Controller")]
	public class BCharacterController : BPairCachingGhostObject
	{
		private KinematicCharacterController m_characterController;
		public float stepHeight = 1f;
		public int upAxis = 1; //0=x, 1=y, 2=z

		public KinematicCharacterController GetKinematicCharacterController()
		{
			return m_characterController;
		}

		protected override void Awake()
		{
			base.Awake();
			m_collisionFlags = BulletSharp.CollisionFlags.CharacterObject;
		}

		internal override bool _BuildCollisionObject()
		{
			WorldController world = GetWorld();

			if (collisionObject != null)
			{
				if (isInWorld && world != null)
				{
					isInWorld = false;
					world.RemoveCollisionObject(this);
				}
			}

			if (transform.localScale != Vector3.one)
			{
				Debug.LogError("The local scale on this collision shape is not one. Bullet physics does not support scaling on a rigid body world transform. Instead alter the dimensions of the CollisionShape.");
			}

			m_collisionShape = GetComponent<BCollisionShape>();
			if (m_collisionShape == null)
			{
				Debug.LogError("There was no collision shape component attached to this BRigidBody. " + name);
				return false;
			}

			if (!(m_collisionShape.GetCollisionShape() is ConvexShape))
			{
				Debug.LogError("The CollisionShape on this BCharacterController was not a convex shape. " + name);
				return false;
			}

			m_collisionShape.GetCollisionShape();
			if (collisionObject == null)
			{
				collisionObject = new PairCachingGhostObject();
				collisionObject.CollisionShape = m_collisionShape.GetCollisionShape();
				collisionObject.CollisionFlags = m_collisionFlags;
				m_characterController = new KinematicCharacterController((PairCachingGhostObject)collisionObject, (ConvexShape)m_collisionShape.GetCollisionShape(), stepHeight, upAxis);
				BulletSharp.Math.Matrix worldTrans;
				BulletSharp.Math.Quaternion q = transform.rotation.ToBullet();
				BulletSharp.Math.Matrix.RotationQuaternion(ref q, out worldTrans);
				worldTrans.Origin = transform.position.ToBullet();
				collisionObject.WorldTransform = worldTrans;
				collisionObject.UserObject = this;
				//world.world.AddCollisionObject(m_collisionObject, CollisionFilterGroups.CharacterFilter, CollisionFilterGroups.StaticFilter | CollisionFilterGroups.DefaultFilter);
				//((DynamicsWorld)world.world).AddAction(m_characterController);
			}
			else
			{
				collisionObject.CollisionShape = m_collisionShape.GetCollisionShape();
				collisionObject.CollisionFlags = m_collisionFlags;
				if (m_characterController != null)
				{
					world.RemoveAction(m_characterController);
				}
				m_characterController = new KinematicCharacterController((PairCachingGhostObject)collisionObject, (ConvexShape)m_collisionShape.GetCollisionShape(), stepHeight, upAxis);
				BulletSharp.Math.Matrix worldTrans;
				BulletSharp.Math.Quaternion q = transform.rotation.ToBullet();
				BulletSharp.Math.Matrix.RotationQuaternion(ref q, out worldTrans);
				worldTrans.Origin = transform.position.ToBullet();
				collisionObject.WorldTransform = worldTrans;
				collisionObject.UserObject = this;
			}
			return true;
		}

		public void Move(Vector3 displacement)
		{
			m_characterController.SetWalkDirection(displacement.ToBullet());
		}

		public void Jump()
		{
			m_characterController.Jump();
		}

		public void Rotate(float turnAmount)
		{
			BulletSharp.Math.Matrix xform = collisionObject.WorldTransform;
			BulletSharp.Math.Matrix orn = xform;
			BulletSharp.Math.Vector3 pos = xform.Origin;
			orn.Row4 = new BulletSharp.Math.Vector4(0, 0, 0, 1);
			BulletSharp.Math.Vector3 upDir = new BulletSharp.Math.Vector3(xform.M21, xform.M22, xform.M23);
			orn *= BulletSharp.Math.Matrix.RotationAxis(upDir, turnAmount);
			orn.Row4 = new BulletSharp.Math.Vector4(pos.X, pos.Y, pos.Z, 1);
			collisionObject.WorldTransform = orn;
		}

		public void FixedUpdate()
		{
			BulletSharp.Math.Matrix trans;
			collisionObject.GetWorldTransform(out trans);
			transform.position = BSExtensionMethods2.ExtractTranslationFromMatrix(ref trans);
			transform.rotation = BSExtensionMethods2.ExtractRotationFromMatrix(ref trans);
			transform.localScale = BSExtensionMethods2.ExtractScaleFromMatrix(ref trans);
		}
	}
}
