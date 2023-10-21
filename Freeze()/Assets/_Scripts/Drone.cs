using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using static utils;

public class Drone : NodeGraphPlayer
{
    public Level level;

    public int hp;
    public int atk;
    public int speed = 1;

    protected Vector3Int currentPos;
    protected Vector3Int? goalPos;
    protected List<Vector3Int> legalPos;
    protected List<float> scores;

    protected bool updateMovement;
    protected int currentCheckPoint = -1;

    protected override void Awake()
    {
        // Initialize the list of actions
        actions["Move"] = Move;
        actions["Chase"] = Chase;
        actions["Retreat"] = Retreat;
        actions["Stay"] = Stay;
        actions["Attack"] = Attack;
        actions["StopAttack"] = StopAttack;
        actions["SendSignal"] = SendSignal;

        conditions["CheckDistance"] = CheckDistance;
        conditions["CheckHp"] = CheckHp;
        conditions["CheckSignal"] = CheckSignal;
        conditions["CheckAlly"] = CheckAlly;
        conditions["CheckEnemy"] = CheckEnemy;
    }

    protected override void Update()
    {
        if (level==null) return;

        //Movement
        PreUpdateMovement();
        base.Update();
        PostUpdateMovement();
    }

    protected virtual List<Vector3Int> GetLegalPos(List<Vector3Int> legalMoves)
    {
        return legalMoves;
    }

    protected void MoveTo(Vector3Int? pos) {
        //Debug.Log(pos);
        if (pos==null) return;
        Vector3 vector = level.GetCellCenter(pos.Value.x, pos.Value.y, pos.Value.z) - transform.position;
        if (vector.magnitude < 0.1f) return;
        transform.position += vector.normalized*0.01f*speed;
    }





    protected void PreUpdateMovement() {
        currentPos = level.GetDronePos(this);
        if (currentPos == goalPos||goalPos == null||ManhattanDistance(currentPos, goalPos.Value)>2) {
            updateMovement = true;
            //Reset goal
            goalPos = null;
            Debug.Log("UpdateMovement");
        } else {
            updateMovement = false;
            return;
        }
        legalPos = new List<Vector3Int>(){currentPos};
        foreach (Vector3Int pos in GetLegalPos(level.GetSuccessors(currentPos))) {
            legalPos.Add(pos);
        }
        scores = new List<float>();
        for (int i=0; i<legalPos.Count; i++) {
            scores.Add(-level.GetCell(legalPos[i]).GetCost());
        }
    }

    protected void PostUpdateMovement() {
        if (updateMovement) {
            int bestMove = scores.IndexOf(scores.Max());
            if (bestMove>0) goalPos = legalPos[bestMove];
        }
        MoveTo(goalPos);
    }

    // Conditions
    public bool CheckDistance(int[] ai)
    {
        // Implement your logic to check distance based on the 'ai' parameters.
        return false;
    }

    public bool CheckHp(int[] ai)
    {
        // Implement your logic to check HP based on the 'ai' parameters.
        return false;
    }

    public bool CheckSignal(int[] ai)
    {
        // Implement your logic to check for signals based on the 'ai' parameters.
        return false;
    }

    public bool CheckAlly(int[] ai)
    {
        // Implement your logic to check for allies based on the 'ai' parameters.
        return false;
    }

    public bool CheckEnemy(int[] ai)
    {
        // Implement your logic to check for enemies based on the 'ai' parameters.
        return false;
    }

    // Functions
    // Movement
    public void Move(int[] ai)
    {
        if (!updateMovement) {
            return;
        }
        // GetFirstCheckpoint
        if (currentCheckPoint==-1) {
            List<int> md = new List<int>();
            for (int i=0; i<ai.Length; i++) {
                md.Add(ManhattanDistance(currentPos, level.checkpoints[i]));
            }
            currentCheckPoint = md.IndexOf(md.Min());
        }
        if (level.GetDronePos(this) == level.checkpoints[currentCheckPoint]) {
            currentCheckPoint = (currentCheckPoint+1)%ai.Length;
        }
        for (int i=0; i<scores.Count; i++) {
            scores[i] -= (int)(ManhattanDistance(legalPos[i], level.checkpoints[currentCheckPoint]));
        }
        Debug.Log("Moving");
        // Implement your Move logic here based on the 'ai' parameters.
    }

    public void Chase(int[] ai)
    {
        if (!updateMovement) {
            return;
        }
        // Implement your Chase logic here based on the 'ai' parameters.
    }

    public void Retreat(int[] ai)
    {
        // Implement your Retreat logic here based on the 'ai' parameters.
    }

    public void Stay(int[] ai)
    {
        // Implement your Stay logic here based on the 'ai' parameters.
    }

    // Battle
    public void Attack(int[] ai)
    {
        // Implement your Attack logic here based on the 'ai' parameters.
    }

    public void StopAttack(int[] ai)
    {
        // Implement your StopAttack logic here based on the 'ai' parameters.
    }

    // Signal
    public void SendSignal(int[] ai)
    {
        // Implement your SendSignal logic here based on the 'ai' parameters.
    }
}
