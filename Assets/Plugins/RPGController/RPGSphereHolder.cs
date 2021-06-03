using UnityEngine;
using System.Collections;

namespace RPGCharacterController
{
	public class RPGSphereHolder : MonoBehaviour
	{
		[SerializeField] Transform target = null;
		[SerializeField] private bool displayGizmos = true;
		public CollisionSphere sphere = new CollisionSphere (0.4f, false, false);
		public float bodyRadius = 1f;


		void OnDrawGizmos ()
		{
			if (displayGizmos)
			{
			
				Gizmos.color = sphere.IsFeet ? Color.green : (sphere.IsHead ? Color.yellow : Color.cyan);
				Gizmos.DrawWireSphere (this.GetSpherePosition (), bodyRadius);	

			}
		}

		public Vector3 GetSpherePosition ()
		{
			Vector3 p;
			if (target == null)
			{
				p = transform.position;
			}
			else
			{
				p = target.position;
			}

			p.y += sphere.yOffset;

			return p;
		}
	}
}