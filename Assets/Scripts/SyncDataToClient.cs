using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SyncDataToClient
{
    public Vector3[] Positions { get; set; } = new Vector3[3];

    public Vector3[] FreeBallPosition { get; set; } = new Vector3[64];
}
