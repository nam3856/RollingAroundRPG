using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TeleportSkill : Skill
{
    public float teleportDistance = 1f;
    private HashSet<int> effectedPlayers = new HashSet<int>();
    public TeleportSkill(List<Skill> prerequisites) : base("텔레포트", "이동 방향으로 순간이동합니다.", 1, prerequisites, 0, 2f) { }

    protected override void ExecuteSkill(Character character)
    {
        Vector2 moveDirection = character.GetLastMoveDirection();

        if (moveDirection == Vector2.zero)
        {
            moveDirection = Vector2.left;
        }

        Vector2 currentPosition = character.transform.position;
        Vector2 targetPosition = currentPosition + moveDirection.normalized * teleportDistance;

        LayerMask collisionLayer = LayerMask.GetMask("Obstacle");

        RaycastHit2D[] hits = Physics2D.RaycastAll(currentPosition, moveDirection.normalized, teleportDistance, collisionLayer);

        if (hits.Length == 0)
        {
            // 충돌이 없으면 목표 위치로 텔레포트
            character.transform.position = targetPosition;
        }
        else
        {
            // 충돌한 모든 콜라이더의 끝 지점을 찾아서 통과 가능한지 확인
            bool canPassThrough = true;
            float distanceCovered = 0f;
            Vector2 lastPosition = currentPosition;

            foreach (var hit in hits)
            {
                Collider2D collider = hit.collider;

                // 콜라이더의 경계 영역 가져오기
                Bounds bounds = collider.bounds;

                // 콜라이더의 반대편 지점 계산
                Vector2 obstacleExitPoint = GetColliderExitPoint(bounds, moveDirection.normalized);

                // 현재 위치에서 장애물의 반대편 지점까지 거리 계산
                float distanceToExit = Vector2.Distance(lastPosition, obstacleExitPoint);

                // 이동 거리에 포함되는지 확인
                if (distanceCovered + distanceToExit > teleportDistance)
                {
                    canPassThrough = false;
                    break;
                }

                // 거리 누적 및 위치 업데이트
                distanceCovered += distanceToExit;
                lastPosition = obstacleExitPoint;
            }

            if (canPassThrough)
            {
                // 장애물을 통과하여 이동
                character.transform.position = targetPosition;
            }
            else
            {
                // 이동 가능한 최대 거리까지만 이동
                Vector2 finalPosition = currentPosition + moveDirection.normalized * (distanceCovered - 0.1f);
                character.transform.position = finalPosition;
            }
        }

        character.transform.position = targetPosition;
    }
    private Vector2 GetColliderExitPoint(Bounds bounds, Vector2 direction)
    {
        // 방향에 따라 콜라이더의 반대편 지점을 계산
        Vector2 exitPoint = Vector2.zero;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // 좌우 이동
            if (direction.x > 0)
            {
                exitPoint = new Vector2(bounds.max.x, bounds.center.y);
            }
            else
            {
                exitPoint = new Vector2(bounds.min.x, bounds.center.y);
            }
        }
        else
        {
            // 상하 이동
            if (direction.y > 0)
            {
                exitPoint = new Vector2(bounds.center.x, bounds.max.y);
            }
            else
            {
                exitPoint = new Vector2(bounds.center.x, bounds.min.y);
            }
        }

        return exitPoint;
    }

}
