namespace RPGCharacterController
{
	[System.Serializable]
	public class CollisionSphere
	{
		public float yOffset;
		public bool IsFeet;
		public bool IsHead;
		//vincentl: Add Vector3 to simplify calculations
		public CollisionSphere ()
		{

		}

		public CollisionSphere (float offset, bool isFeet, bool isHead)
		{
			yOffset = offset;
			IsFeet = isFeet;
			IsHead = isHead;
		}
	}
}