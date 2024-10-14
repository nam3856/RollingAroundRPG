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
    public TeleportSkill(List<Skill> prerequisites) : base("�ڷ���Ʈ", "�̵� �������� �����̵��մϴ�.", 1, prerequisites, 0, 2f) { }

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
            // �浹�� ������ ��ǥ ��ġ�� �ڷ���Ʈ
            character.transform.position = targetPosition;
        }
        else
        {
            // �浹�� ��� �ݶ��̴��� �� ������ ã�Ƽ� ��� �������� Ȯ��
            bool canPassThrough = true;
            float distanceCovered = 0f;
            Vector2 lastPosition = currentPosition;

            foreach (var hit in hits)
            {
                Collider2D collider = hit.collider;

                // �ݶ��̴��� ��� ���� ��������
                Bounds bounds = collider.bounds;

                // �ݶ��̴��� �ݴ��� ���� ���
                Vector2 obstacleExitPoint = GetColliderExitPoint(bounds, moveDirection.normalized);

                // ���� ��ġ���� ��ֹ��� �ݴ��� �������� �Ÿ� ���
                float distanceToExit = Vector2.Distance(lastPosition, obstacleExitPoint);

                // �̵� �Ÿ��� ���ԵǴ��� Ȯ��
                if (distanceCovered + distanceToExit > teleportDistance)
                {
                    canPassThrough = false;
                    break;
                }

                // �Ÿ� ���� �� ��ġ ������Ʈ
                distanceCovered += distanceToExit;
                lastPosition = obstacleExitPoint;
            }

            if (canPassThrough)
            {
                // ��ֹ��� ����Ͽ� �̵�
                character.transform.position = targetPosition;
            }
            else
            {
                // �̵� ������ �ִ� �Ÿ������� �̵�
                Vector2 finalPosition = currentPosition + moveDirection.normalized * (distanceCovered - 0.1f);
                character.transform.position = finalPosition;
            }
        }

        character.transform.position = targetPosition;
    }
    private Vector2 GetColliderExitPoint(Bounds bounds, Vector2 direction)
    {
        // ���⿡ ���� �ݶ��̴��� �ݴ��� ������ ���
        Vector2 exitPoint = Vector2.zero;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // �¿� �̵�
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
            // ���� �̵�
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
