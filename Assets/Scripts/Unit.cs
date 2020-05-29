using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;

	public Transform target;
	public float speed = 15;
	public float turnSpeed = 3;
	public float turnDst = 5;
	public float stoppingDst = 10;

	//used for fast approach
	Path path;

	//Used for slow approach
	//Vector3[] path;
	//int targetIndex;

	void Start()
	{
		StartCoroutine(UpdatePath());
	}
	public void OnPathFound(Vector3[] wayPoints, bool pathSuccessful)
	{
		if (pathSuccessful)
		{
			path = new Path(wayPoints, transform.position, turnDst,stoppingDst);
	
			StopCoroutine(FollowPath());
			StartCoroutine(FollowPath());
		}
	}

	IEnumerator UpdatePath()
	{

		if (Time.timeSinceLevelLoad < .3f)
		{
			yield return new WaitForSeconds(.3f);
		}

		//Without threading
		//PathManagerWithThreading.RequestPath(transform.position, target.position, OnPathFound);

		//With Threading
		PathManagerWithThreading.RequestPath(new PathRequestForThreading(transform.position, target.position, OnPathFound));

		float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
		Vector3 targetPosOld = target.position;

		while (true)
		{
			yield return new WaitForSeconds(minPathUpdateTime);
			if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
			{
				//Without threading
				//PathManagerWithThreading.RequestPath(transform.position, target.position, OnPathFound);

				//With Threading
				PathManagerWithThreading.RequestPath(new PathRequestForThreading(transform.position, target.position, OnPathFound));
				targetPosOld = target.position;
			}
		}
	}

	#region Pathfinding Slow Approach without smoothing
	//One way to  perform pathfinding(Inefficient)
	//IEnumerator FollowPath()
	//{
	//	Vector3 currentWaypoint = path[0];
	//	while (true)
	//	{
	//		if (transform.position == currentWaypoint)
	//		{
	//			targetIndex++;
	//			if (targetIndex >= path.Length)
	//			{
	//				yield break;
	//			}
	//			currentWaypoint = path[targetIndex];
	//		}

	//		transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
	//		yield return null;

	//	}
	//}
	#endregion

	#region Pathfinding Fast Approach with path smoothing
	IEnumerator FollowPath()
	{

		bool followingPath = true;
		int pathIndex = 0;
		transform.LookAt(path.lookPoints[0]);

		float speedPercent = 1;

		while (followingPath)
		{
			Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
			while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
			{
				if (pathIndex == path.finishLineIndex)
				{
					followingPath = false;
					break;
				}
				else
				{
					pathIndex++;
				}
			}

			if (followingPath)
			{

				if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
				{
					speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
					if (speedPercent < 0.01f)
					{
						followingPath = false;
					}
				}

				Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
				transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
			}

			yield return null;

		}
	}
    #endregion

    public void OnDrawGizmos()
	{
		if (path != null)
		{
			path.DrawWithGizmos();
		}
	}
}
