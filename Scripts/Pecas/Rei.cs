using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rei : XadrezPeca
{
    public override List<Vector2Int> GetAvaliabeMoves(ref XadrezPeca[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // direita
        if (currentX + 1 < tileCountX)
        {
            if (board[currentX + 1, currentY] == null)
                r.Add(new Vector2Int(currentX + 1, currentY));
            else if (board[currentX + 1, currentY].team != team)
                r.Add(new Vector2Int(currentX + 1, currentY));

            // top right
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX + 1, currentY + 1] == null)
                    r.Add(new Vector2Int(currentX + 1, currentY + 1));
                else if (board[currentX + 1, currentY + 1].team != team)
                    r.Add(new Vector2Int(currentX + 1, currentY + 1));
            }
            // baixo direita
            if (currentY - 1 >= 0)
            {
                if (board[currentX + 1, currentY - 1] == null)
                    r.Add(new Vector2Int(currentX + 1, currentY - 1));
                else if (board[currentX + 1, currentY - 1].team != team)
                    r.Add(new Vector2Int(currentX + 1, currentY - 1));
            }
        }

        // esquerda
        if (currentX - 1 >=  0)
        {
            if (board[currentX - 1, currentY] == null)
                r.Add(new Vector2Int(currentX -1, currentY));
            else if (board[currentX - 1, currentY].team != team)
                r.Add(new Vector2Int(currentX - 1, currentY));

            // top right
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX - 1, currentY + 1] == null)
                    r.Add(new Vector2Int(currentX - 1, currentY + 1));
                else if (board[currentX - 1, currentY + 1].team != team)
                    r.Add(new Vector2Int(currentX - 1, currentY + 1));
            }
            // baixo direita
            if (currentY - 1 >= 0)
            {
                if (board[currentX - 1, currentY - 1] == null)
                    r.Add(new Vector2Int(currentX - 1, currentY - 1));
                else if (board[currentX - 1, currentY - 1].team != team)
                    r.Add(new Vector2Int(currentX - 1, currentY - 1));
            }
        }
        // cima
        if(currentY + 1 < tileCountY)
            if(board[currentX, currentY + 1] == null || board[currentX, currentY + 1].team != team)
                r.Add(new Vector2Int(currentX, currentY + 1));

        // baixo
        if(currentY - 1 >=0)
           if(board[currentX, currentY - 1] == null || board[currentX, currentY -1].team != team)
             r.Add(new Vector2Int(currentX, currentY -1 ));

        return r;
    }
}
