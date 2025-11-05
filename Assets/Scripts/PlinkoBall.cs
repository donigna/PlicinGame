using UnityEngine;
using System.Collections.Generic;

public class PlinkoBall : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float bounceHeight = 0.3f;
    public float horizontalStep = 0.5f;
    public float reachThreshold = 0.02f;
    public float bounceSmoothTime = 0.06f;
    public float fallSmoothTime = 0.1f;
    public float finalSettleDrop = 1f;
    public float finalBounceHeight = 0.2f;
    public float finalSmoothTime = 0.08f;

    private List<Transform> pathNodes = new List<Transform>();
    private int currentNode = 0;
    private Vector3 targetPos;
    private bool moving = false;
    private bool bouncingUp = false;
    private Vector3 bounceTarget;
    private Vector3 bounceVelocity = Vector3.zero;
    private Vector3 fallVelocity = Vector3.zero;

    private bool finalDropping = false;
    private bool finalBouncingUp = false;
    private bool finalFallingToCenter = false;
    private Vector3 finalBounceTarget;
    private Vector3 finalVelocity = Vector3.zero;
    private PlinkoSlot landedSlot;
    private Vector3 slotCenterPos;

    public void SetPathNodes(List<Transform> nodes)
    {
        pathNodes = nodes;
        if (pathNodes.Count == 0) return;

        currentNode = 0;
        targetPos = pathNodes[0].position;
        moving = true;
    }

    void Update()
    {
        if (!moving && !finalDropping && !finalBouncingUp && !finalFallingToCenter) return;

        // Handle final settle drop and bounce before slot execution
        if (finalDropping)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref finalVelocity,
                finalSmoothTime,
                Mathf.Infinity,
                Time.deltaTime
            );
            if (Vector3.Distance(transform.position, targetPos) < reachThreshold)
            {
                finalDropping = false;
                finalBouncingUp = true;
                // Bounce at the impact X/Z (current position), not at slot center
                finalBounceTarget = new Vector3(transform.position.x, transform.position.y + finalBounceHeight, transform.position.z);
                finalVelocity = Vector3.zero;
            }
            return;
        }

        if (finalBouncingUp)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                finalBounceTarget,
                ref finalVelocity,
                finalSmoothTime,
                Mathf.Infinity,
                Time.deltaTime
            );
            if (Vector3.Distance(transform.position, finalBounceTarget) < reachThreshold)
            {
                finalBouncingUp = false;
                // After bounce apex, smoothly settle to exact slot center, then execute
                finalFallingToCenter = true;
                targetPos = slotCenterPos;
                finalVelocity = Vector3.zero;
            }
            return;
        }

        if (finalFallingToCenter)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref finalVelocity,
                finalSmoothTime,
                Mathf.Infinity,
                Time.deltaTime
            );
            if (Vector3.Distance(transform.position, targetPos) < reachThreshold)
            {
                // Snap to exact center
                transform.position = slotCenterPos;
                finalFallingToCenter = false;
                CheckSlot();
            }
            return;
        }

        if (bouncingUp)
        {
            // Short smooth upward bounce to simulate impact response
            transform.position = Vector3.SmoothDamp(
                transform.position,
                bounceTarget,
                ref bounceVelocity,
                bounceSmoothTime,
                Mathf.Infinity,
                Time.deltaTime
            );
            if (Vector3.Distance(transform.position, bounceTarget) < reachThreshold)
            {
                bouncingUp = false;
                // After the bounce apex, start a smooth fall towards the next node
                Vector3 nextNode = pathNodes[currentNode].position;
                targetPos = nextNode;
                // reset fall velocity for a clean SmoothDamp towards next node
                fallVelocity = Vector3.zero;
            }
        }
        else
        {
            // Smooth fall towards target node
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref fallVelocity,
                fallSmoothTime,
                Mathf.Infinity,
                Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < reachThreshold)
            {
                currentNode++;
                if (currentNode < pathNodes.Count)
                {
                    // prepare a small upward bounce at the impact x
                    bounceTarget = new Vector3(targetPos.x, transform.position.y + bounceHeight, transform.position.z);
                    bouncingUp = true;
                    bounceVelocity = Vector3.zero;
                }
                else
                {
                    // Reached the final node: compute target slot center and run final settle sequence
                    PlinkoManager manager = FindAnyObjectByType<PlinkoManager>();
                    if (manager != null && manager.slots.Count > 0 && manager.pinsByRow.Count > 0)
                    {
                        int lastIndex = manager.pinsByRow[manager.pinsByRow.Count - 1].IndexOf(pathNodes[pathNodes.Count - 1]);
                        lastIndex = Mathf.Clamp(lastIndex, 0, manager.slots.Count - 1);
                        landedSlot = manager.slots[lastIndex];
                        slotCenterPos = landedSlot.transform.position;
                    }
                    else
                    {
                        // Fallback to current x if manager info missing
                        slotCenterPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                    }
                    // Start final drop at current X/Z; centering happens after the bounce
                    targetPos = new Vector3(transform.position.x, transform.position.y - finalSettleDrop, transform.position.z);
                    moving = false;
                    finalDropping = true;
                }
            }
        }
    }

    void CheckSlot()
    {
        PlinkoManager manager = FindAnyObjectByType<PlinkoManager>();
        if (manager == null || manager.slots.Count == 0) return;

        int lastIndex = manager.pinsByRow[manager.pinsByRow.Count - 1].IndexOf(pathNodes[pathNodes.Count - 1]);
        PlinkoSlot targetSlot = manager.slots[lastIndex];
        landedSlot = targetSlot;

        Debug.Log("Bola mendarat di slot multiplier: " + targetSlot.multiplier);

        // Check if this is the last ball before destroying
        bool isLastBall = manager.ballsParent.childCount == 1;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddByMultiplier(targetSlot.multiplier, isLastBall);
        }

        Destroy(gameObject);
    }
}
