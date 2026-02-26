using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Utilities.Extensions
{
	public static class NavMeshAgentExtensions
	{
		/// Moves the agent to the given destination, invoking the callback once it arrives.
		/// Any existing path is immediately cleared so your new SetDestination() always takes effect, please avoid calling on same destination while agent is reaching that that destination.
		public static async Task SetDestinationAsync(this NavMeshAgent agent, Vector3 destination, Action onArrived = null, CancellationToken token = default)
		{
			// If we've already set this as the NavMeshAgent.destination, don't re-queue it.
			if (agent.hasPath && agent.destination == destination) return;
			
			// 1) Clear any existing path so we don't have to wait for it
			agent.ResetPath();

			if (!agent.SetDestination(destination))
			{
				Debug.LogWarning($"[{agent.name}] failed to SetDestination({destination})");
				return;
			}
			
			// ensure we yield at least once before hitting the arrival check & callback.
			await Task.Yield();

			// 3) Poll every frame until we arrive or the token is cancelled.
			try
			{
				while (Application.isPlaying && !agent.HasReachedDestination())
				{
					token.ThrowIfCancellationRequested(); // Honor external cancellation.
					await Task.Yield();
				}
				
				// ★ Optionally yield one more time before firing callback
				await Task.Yield();

				if (Application.isPlaying) onArrived?.Invoke();
			}
			catch (OperationCanceledException)
			{
				Debug.Log($"[{agent.name}] navigation to {destination} was cancelled.");
			}
			catch (Exception ex)
			{
				Debug.LogError($"[{agent.name}] error during navigation: {ex}");
			}
		}
		
		public static bool HasReachedDestination(this NavMeshAgent agent) => agent.isActiveAndEnabled && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);

		public static Vector3? SetRandomDestination(this NavMeshAgent agent, float radius, Vector3? origin = null, int areaMask = NavMesh.AllAreas)
		{
			if (!agent) return null;

			for (int i = 0; i < 10; i++)
			{
				Vector3 randomDirection = Random.insideUnitSphere * radius;
				randomDirection += origin ?? agent.transform.position;
				if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, areaMask))
				{
					agent.SetDestination(hit.position);
					return hit.position;
				}
			}

			return null;
		}

		public static NavMeshAgent SmoothSpeedChange(this NavMeshAgent agent, MonoBehaviour monoBehaviour, float targetSpeed, float duration)
		{
			float startSpeed = agent.speed;
			float elapsedTime = 0f;
			monoBehaviour.StartCoroutine(Routine());

			return agent;

			IEnumerator Routine()
			{
				while (elapsedTime < duration)
				{
					agent.speed = Mathf.Lerp(startSpeed, targetSpeed, elapsedTime / duration);
					elapsedTime += Time.deltaTime;
					yield return null;
				}

				agent.speed = targetSpeed;
			}
		}

		public static NavMeshAgent PatrolDestination(this NavMeshAgent agent, List<Vector3> patrolPath, float tolerance = 1f)
		{
			if (patrolPath == null || patrolPath.Count == 0) return null;

			Vector3 currentWaypoint = patrolPath[0];
			if (Vector3.Distance(agent.transform.position, currentWaypoint) <= tolerance)
			{
				int nextIndex = (patrolPath.IndexOf(currentWaypoint) + 1) % patrolPath.Count;
				currentWaypoint = patrolPath[nextIndex];
			}

			agent.SetDestination(currentWaypoint);
			return agent;
		}

		/// <summary>
		/// Call it from a loop
		/// </summary>
		public static NavMeshAgent PatrolDestination(this NavMeshAgent agent, List<Transform> patrolPath, float tolerance = 1f)
		{
			if (patrolPath == null || patrolPath.Count == 0) return null;

			Transform currentWaypoint = patrolPath[0];
			if (Vector3.Distance(agent.transform.position, currentWaypoint.position) <= tolerance)
			{
				int nextIndex = (patrolPath.IndexOf(currentWaypoint) + 1) % patrolPath.Count;
				currentWaypoint = patrolPath[nextIndex];
			}

			agent.SetDestination(currentWaypoint.position);
			return agent;
		}

		public static NavMeshAgent AddKnockBack(this NavMeshAgent agent, Transform target, float force)
		{
			agent.ResetPath();
			Vector3 selfPosition = agent.transform.position;
			Vector3 knockBackDirection = target.position.WithY(0) - selfPosition.WithY(0);
			agent.SetDestination(selfPosition + knockBackDirection.normalized * force);
			return agent;
		}

		public static NavMeshAgent SetTemporarySpeed(this NavMeshAgent agent, MonoBehaviour monoBehaviour, float temporarySpeed, float duration)
		{
			if (agent == null || !agent.isActiveAndEnabled) return null;
			
			monoBehaviour.StartCoroutine(TemporarySpeedCoroutine(agent, temporarySpeed, duration));

			return agent;

			IEnumerator TemporarySpeedCoroutine(NavMeshAgent agent, float temporarySpeed, float duration)
			{
				float originalSpeed = agent.speed;
				agent.speed = temporarySpeed;

				yield return new WaitForSeconds(duration);

				if (agent != null && agent.isActiveAndEnabled)
				{
					agent.speed = originalSpeed;
				}
			}
		}

		public static IEnumerator Wander(this NavMeshAgent agent, Func<float> radius, Func<float> waitTime = null, Func<bool> loopWhile = null, Action OnStartMoving = null, Action OnStopMoving = null, Action OnUpdate = null)
		{
			if (agent == null || !agent.isActiveAndEnabled)
				yield break;
			
			while (agent && !agent.isStopped && agent.isActiveAndEnabled && (loopWhile?.Invoke() ?? true))
			{
				if (agent.SetRandomDestination(radius()) != null)
				{
					OnStartMoving?.Invoke();
					yield return new WaitUntil(agent.HasReachedDestination);
					OnStopMoving?.Invoke();
				}

				yield return new WaitForSeconds(waitTime?.Invoke() ?? 1);
				OnUpdate?.Invoke();
			}
		}

		public static NavMeshAgent Chase(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<Transform> target, Func<float> minDistanceKeep, Func<float> maxDistanceKeep, Func<float> delayBetweenSettingDestination = null, Func<bool> loopWhile = null, Func<float> distanceToPlayer = null, Action OnUpdate = null)
		{
			if (agent == null || !agent.isActiveAndEnabled || target == null)
				return null;
			monoBehaviour.StartCoroutine(ChaseTargetCoroutine());

			return agent;

			IEnumerator ChaseTargetCoroutine()
			{
				Vector3 selfPosition = agent.transform.position;

				while (agent && agent.isActiveAndEnabled && (loopWhile?.Invoke() ?? true))
				{
					WaitForSeconds delay = new WaitForSeconds(delayBetweenSettingDestination?.Invoke() ?? 0);
					float distance = distanceToPlayer?.Invoke() ?? agent.transform.Distance(target());

					if (minDistanceKeep == null || maxDistanceKeep == null)
					{
						agent.SetDestination(target().position);
					}
					else
					{
						if (distance < minDistanceKeep())
						{
							Vector3 directionToMoveWhenTooCloseToPlayer = (target().position - selfPosition).normalized;
							Vector3 positionToMove = selfPosition + directionToMoveWhenTooCloseToPlayer * (-1 * (maxDistanceKeep() - distance));
							NavMeshPath path = new NavMeshPath();
							if (agent.CalculatePath(positionToMove, path)) agent.SetDestination(positionToMove);
						}
						else
						{
							agent.SetDestination(target().position);
						}
					}

					if (delayBetweenSettingDestination == null)
						yield return null;
					else
						yield return delay;

					OnUpdate?.Invoke();
				}
			}
		}

		public static NavMeshAgent Flee(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<Transform> target = null, Func<float> fleeDistance = null, Func<bool> loopWhile = null, Action OnUpdate = null)
		{
			if (!agent || !agent.isActiveAndEnabled || target == null) return null;
			
			monoBehaviour.StartCoroutine(FleeFromTargetCoroutine());

			return agent;

			IEnumerator FleeFromTargetCoroutine()
			{
				while (agent != null && agent.isActiveAndEnabled && target != null && (loopWhile?.Invoke() ?? true))
				{
					float fleeDistanceValue = fleeDistance?.Invoke() ?? 10;
					Vector3 fleeDirection = (agent.transform.position - target().position).normalized;
					Vector3 fleePosition = agent.transform.position + fleeDirection * fleeDistanceValue;

					if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, fleeDistanceValue, NavMesh.AllAreas))
					{
						agent.SetDestination(hit.position);
					}

					yield return null;
					OnUpdate?.Invoke();
				}
			}
		}

		public static NavMeshAgent Patroll(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<List<Transform>> waypoints, float tolerance = 0.1f, Func<float> waitTime = null, Func<bool> followWaypointOrder = null, Func<bool> loopWhile = null, Action OnStartMoving = null, Action OnStopMoving = null, Action OnUpdate = null)
		{
			monoBehaviour.StartCoroutine(PatrolWaypointsCoroutine());

			return agent;

			IEnumerator PatrolWaypointsCoroutine()
			{
				int currentWaypointIndex = 0;

				while (agent && agent.isActiveAndEnabled && (loopWhile?.Invoke() ?? true))
				{
					Transform currentWaypoint = waypoints()[currentWaypointIndex];
					agent.SetDestination(currentWaypoint.position);
					OnStartMoving?.Invoke();
					yield return new WaitUntil(() => agent.remainingDistance <= tolerance);
					OnStopMoving?.Invoke();
					yield return new WaitForSeconds(waitTime?.Invoke() ?? 1);
					currentWaypointIndex = followWaypointOrder?.Invoke() ?? true ? (currentWaypointIndex + 1) % waypoints().Count : Random.Range(0, waypoints().Count);
					OnUpdate?.Invoke();
				}
			}
		}

		public static NavMeshAgent ContinuesAvoidObstaclesWhile(this NavMeshAgent agent, LayerMask obstacleMask, float avoidanceRadius, MonoBehaviour monoBehaviour, Func<bool> condition = null)
		{
			if (!agent || !agent.isActiveAndEnabled) return null;
			
			monoBehaviour.StartCoroutine(AvoidObstaclesCoroutine());

			return agent;

			IEnumerator AvoidObstaclesCoroutine()
			{
				while (agent && agent.isActiveAndEnabled && (condition?.Invoke() ?? true))
				{
					var obstacles = Physics.OverlapSphere(agent.transform.position, avoidanceRadius, obstacleMask);
					if (obstacles.Length > 0)
					{
						Vector3 avoidanceDirection = obstacles.Aggregate(Vector3.zero, (current, obstacle) => current + (agent.transform.position - obstacle.transform.position).normalized);

						avoidanceDirection /= obstacles.Length;

						Vector3 newDestination = agent.transform.position + avoidanceDirection * avoidanceRadius;
						if (NavMesh.SamplePosition(newDestination, out NavMeshHit hit, avoidanceRadius, NavMesh.AllAreas))
						{
							agent.SetDestination(hit.position);
						}
					}

					yield return null;
				}
			}
		}

		// public static void WaitAtDestination(this NavMeshAgent agent, Func<bool> condition, float waitTime, MonoBehaviour monoBehaviour)
		// {
		//     if (!agent || !agent.isActiveAndEnabled) return;
		//     monoBehaviour.StartCoroutine(WaitAtDestinationCoroutine());
		//
		//     IEnumerator WaitAtDestinationCoroutine()
		//     {
		//         agent.isStopped = true;
		//         yield return new WaitForSeconds(waitTime);
		//         if (agent != null && agent.isActiveAndEnabled)
		//         {
		//             agent.isStopped = false;
		//         }
		//     }
		// }
	}
}