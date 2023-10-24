using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Funnel
{
    public static List<Vector2> GetPath(Vector2 agent, List<Edge> edges, List<Triangle> triangles, int depth=999) {
        List<Vector2> lst = new List<Vector2>();
        Vector2 funnel1 = edges[0].Point1; //clockwise agent
        Vector2 funnel2 = edges[0].Point2; //counter agent
        Vector2 v1 = funnel1 - agent;
        Vector2 v2 = funnel2 - agent;
        float angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), Vector3.forward);
        if (angle>0) {
            Vector2 v = funnel1;
            funnel1 = funnel2;
            funnel2 = v;
        }
        int index1 = 0;
        int index2 = 0;
        bool end1 = false;
        bool end2 = false;
        bool blocked1 = false;
        bool blocked2 = false;

        DebugShapes.DrawSphere(new Vector3(funnel1.x, funnel1.y, 0), 0.2f, Color.cyan);
        DebugShapes.DrawSphere(new Vector3(funnel2.x, funnel2.y, 0), 0.2f, Color.green);


        int n=0;
        while (n<4) {

        if (!blocked1&&!end1) {
            Debug.Log(n);

            int newIndex = index1;
            Vector2 newFunnel = funnel1;
            NextFunnel(ref newIndex, ref newFunnel, edges, triangles);
            Debug.Log(newIndex);
            
            v1 = funnel1 - agent;
            v2 = newFunnel - agent;
            angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), Vector3.forward);
            if ((angle>0)==true) {
                blocked1 = true;
                Debug.Log("blocked1");
                index1 = newIndex-1;
            } else {
                v1 = newFunnel - agent;
                v2 = funnel2 - agent;
                angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), Vector3.forward);
                //DebugShapes.DrawSphere(new Vector3(newFunnel.x, newFunnel.y, 0), 0.7f, Color.cyan);
                //DebugShapes.DrawSphere(new Vector3(funnel2.x, funnel2.y, 0), 0.7f, Color.green);
                if (angle>0) {
                    Debug.Log("intersect1");
                    if (end2) {
                        agent = newFunnel;
                        Edge lastEdge = edges[edges.Count-1];
                        while(lastEdge.Point1!=agent&&lastEdge.Point2!=agent) {
                            NextFunnel(ref newIndex, ref newFunnel, edges, triangles);
                            agent = newFunnel;
                        }
                        lst.Add(agent);
                        break;
                    } else {
                        agent = funnel2;
                        DebugShapes.DrawSphere(new Vector3(agent.x, agent.y, 0), 0.6f, Color.red);
                        lst.Add(agent);
                        Edge last = edges[edges.Count-1];
                        if (last.Point1==agent||last.Point2==agent) {
                            break;
                        }
                        NextFunnel(ref index2, ref funnel2, edges, triangles);
                        Edge newEdge = edges[index2];
                        funnel1 = (newEdge.Point2==funnel2)?newEdge.Point1:newEdge.Point2;
                        index1 = index2;
                        Debug.Log(index1);
                        blocked1 = false;
                        blocked2 = false;
                    }
                } else {
                    Debug.Log("move1");
                    funnel1 = newFunnel;
                    index1 = newIndex;
                    DebugShapes.DrawSphere(new Vector3(funnel1.x, funnel1.y, 0), 0.5f, Color.cyan);
                }
            }
            if (index1>=edges.Count) {
                end1 = true;
                Debug.Log("end1");
            }
        }
        DebugShapes.DrawSphere(new Vector3(funnel1.x, funnel1.y, 0), 0.3f, Color.cyan);
        DebugShapes.DrawSphere(new Vector3(funnel2.x, funnel2.y, 0), 0.3f, Color.green);

        if (!blocked2&&!end2) {
            int newIndex = index2;
            Vector2 newFunnel = funnel2;
            NextFunnel(ref newIndex, ref newFunnel, edges, triangles);
            //Debug.Log(newIndex);
            
            v1 = funnel2 - agent;
            v2 = newFunnel - agent;
            angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), Vector3.forward);
            if ((angle<0)==true) {
                blocked2 = true;
                Debug.Log("blocked2");
                index2 = newIndex-1;
            } else {
                index2 = newIndex;
                v1 = funnel1 - agent;
                v2 = newFunnel - agent;
                angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), Vector3.forward);
                //DebugShapes.DrawSphere(new Vector3(newFunnel.x, newFunnel.y, 0), 0.7f, Color.cyan);
                //DebugShapes.DrawSphere(new Vector3(funnel2.x, funnel2.y, 0), 0.7f, Color.green);
                if (angle>0) {
                    Debug.Log("intersect2");
                    if (end1) {
                        agent = newFunnel;
                        Edge lastEdge = edges[edges.Count-1];
                        while(lastEdge.Point1!=agent&&lastEdge.Point2!=agent) {
                            NextFunnel(ref newIndex, ref newFunnel, edges, triangles);
                            agent = newFunnel;
                        }
                        lst.Add(agent);
                        break;
                    } else {
                        agent = funnel1;
                        DebugShapes.DrawSphere(new Vector3(agent.x, agent.y, 0), 0.6f, Color.red);
                        lst.Add(agent);
                        Edge last = edges[edges.Count-1];
                        if (last.Point1==agent||last.Point2==agent) {
                            break;
                        }
                        NextFunnel(ref index1, ref funnel1, edges, triangles);
                        Edge newEdge = edges[index1];
                        blocked1 = false;
                        blocked2 = false;
                        funnel2 = (newEdge.Point2==funnel1)?newEdge.Point1:newEdge.Point2;
                    }
                } else {
                    Debug.Log("move2");
                    funnel2 = newFunnel;
                }
            }
            if (index2>=edges.Count) {
                end2 = true;
                Debug.Log("end2");
            }
        }
        DebugShapes.DrawSphere(new Vector3(funnel1.x, funnel1.y, 0), 0.3f, Color.cyan);
        DebugShapes.DrawSphere(new Vector3(funnel2.x, funnel2.y, 0), 0.3f, Color.green);
        n++;
        }

        return lst;
    }
    /*
    public static List<Vector2> GetPath(Vector2 agent, List<Edge> edges, List<Triangle> triangles, int depth=999) {
        List<Vector2> lst = new List<Vector2>();
        //Debug.Log(triangles.Count);
        //Debug.Log(edges.Count);
        Vector2 funnel1 = edges[0].Point1; //clockwise agent
        Vector2 funnel2 = edges[0].Point2; //counter agent
        int index1 = 0;
        int index2 = 0;
        Vector2 v1 = funnel1 - agent;
        Vector2 v2 = funnel2 - agent;
        bool end1 = false;
        bool end2 = false;
        bool terminate1 = false;
        bool terminate2 = false;
        float angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), Vector3.forward);
        if (angle>0) {
            Vector2 v = funnel1;
            funnel1 = funnel2;
            funnel2 = v;
        }
        DebugShapes.DrawSphere(new Vector3(funnel1.x, funnel1.y, 0), 0.2f, Color.cyan);
        DebugShapes.DrawSphere(new Vector3(funnel2.x, funnel2.y, 0), 0.2f, Color.green);
        int n = 0;
        while(index1<edges.Count&&n<2) {
            Debug.Log(n);
            if (!end1) {
                int newIndex = index1;
                Vector2 newFunnel = funnel1;
                NextFunnel(ref newIndex, ref newFunnel, edges, triangles);
                v1 = funnel1 - agent;
                v2 = newFunnel - agent;
                angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), Vector3.forward);
                //DebugShapes.DrawSphere(new Vector3(newFunnel.x, newFunnel.y, 0), 0.2f, Color.cyan);
                if ((angle>0)==true||newIndex>=edges.Count) {
                    Debug.Log("end");
                    index1 = newIndex-1;
                    Debug.Log(index1);
                    end1 = true;
                    Debug.Log(end2);
                    if (end2) {
                        end1 = false;
                        end2 = false;
                        agent = funnel2;
                        lst.Add(agent);
                        DebugShapes.DrawSphere(new Vector3(agent.x, agent.y, 0), 0.5f, Color.red);
                        NextFunnel(ref index2, ref funnel2, edges, triangles);
                        if (newIndex>=edges.Count) {
                            break;
                        }
                        Edge newEdge = edges[index2];
                        funnel1 = (newEdge.Point2==funnel2)?newEdge.Point1:newEdge.Point2;
                        Debug.Log(index1);
                        Debug.Log(index2);
                    }
                } else {
                    index1 = newIndex;
                    v1 = newFunnel - agent;
                    v2 = funnel2 - agent;
                    angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), Vector3.forward);
                    //DebugShapes.DrawSphere(new Vector3(newFunnel.x, newFunnel.y, 0), 0.7f, Color.cyan);
                    //DebugShapes.DrawSphere(new Vector3(funnel2.x, funnel2.y, 0), 0.7f, Color.green);
                    if (angle>0) {
                        Debug.Log("intersect");
                        agent = funnel2;
                        lst.Add(agent);
                        NextFunnel(ref index2, ref funnel2, edges, triangles);
                        if (newIndex>=edges.Count) {
                            break;
                        }
                        Edge newEdge = edges[index2];
                        funnel1 = (newEdge.Point2==funnel2)?newEdge.Point1:newEdge.Point2;
                        DebugShapes.DrawSphere(new Vector3(agent.x, agent.y, 0), 0.5f, Color.red);
                    } else {
                        funnel1 = newFunnel;
                    }
                }
            }
            DebugShapes.DrawSphere(new Vector3(funnel1.x, funnel1.y, 0), 0.3f, Color.cyan);

            if (!end2) {
                int newIndex = index2;
                Vector2 newFunnel = funnel2;
                NextFunnel(ref newIndex, ref newFunnel, edges, triangles);
                v1 = funnel2 - agent;
                v2 = newFunnel - agent;
                angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), Vector3.forward);
                if ((angle>0)==false||newIndex>=edges.Count) {
                    index2 = newIndex-1;
                    Debug.Log("end");
                    end2 = true;
                    if (end1) {
                        end1 = false;
                        end2 = false;
                        agent = funnel1;
                        lst.Add(agent);
                        NextFunnel(ref index1, ref funnel1, edges, triangles);
                        if (newIndex>=edges.Count) {
                            break;
                        }
                        Edge newEdge = edges[index1];
                        funnel2 = (newEdge.Point2==funnel1)?newEdge.Point1:newEdge.Point2;
                
                    }
                } else {
                    index2 = newIndex;
                    v1 = funnel1 - agent;
                    v2 = newFunnel - agent;
                    angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), Vector3.forward);
                    if (angle>0) {
                        Debug.Log("intersect");
                        agent = funnel1;
                        lst.Add(agent);
                        NextFunnel(ref index1, ref funnel1, edges, triangles);
                        if (newIndex>=edges.Count) {
                            break;
                        }
                        Edge newEdge = edges[index1];
                        funnel2 = (newEdge.Point2==funnel1)?newEdge.Point1:newEdge.Point2;
                        
                    } else {
                        funnel2 = newFunnel;
                    }
                }
            }
                
            DebugShapes.DrawSphere(new Vector3(funnel1.x, funnel1.y, 0), 0.3f, Color.cyan);
            DebugShapes.DrawSphere(new Vector3(funnel2.x, funnel2.y, 0), 0.3f, Color.green);
            n++;
        }
        return lst;
    }*/

    /*
    public static List<Vector2> GetPath(Vector2 agent, List<Edge> edges, List<Triangle> triangles, int depth=999) {
        List<Vector2> lst = new List<Vector2>();
        //Debug.Log(triangles.Count);
        //Debug.Log(edges.Count);
        Vector2 funnel1 = edges[0].Point1; //clockwise agent
        Vector2 funnel2 = edges[0].Point2; //counter agent
        int index1 = 0;
        int index2 = 0;
        Vector2 v1 = funnel1 - agent;
        Vector2 v2 = funnel2 - agent;
        bool end1 = false;
        bool end2 = false;
        float angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), -Vector3.forward);
        if (angle>0) {
            Vector2 v = funnel1;
            funnel1 = funnel2;
            funnel2 = v;
        }
        DebugShapes.DrawSphere(new Vector3(funnel1.x, funnel1.y, 0), 0.2f, Color.cyan);
        DebugShapes.DrawSphere(new Vector3(funnel2.x, funnel2.y, 0), 0.2f, Color.green);
        int n = 0;
        while(index1<edges.Count&&n<5) {
            if (!end1) {
                int newIndex = index1;
                Vector2 newFunnel = funnel1;
                NextFunnel(ref newIndex, ref newFunnel, edges, triangles);
                v1 = funnel1 - agent;
                v2 = newFunnel - agent;
                Debug.Log(newIndex);
                angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), -Vector3.forward);
                if ((angle>0)!=true) {
                    v1 = newFunnel - agent;
                    v2 = funnel2 - agent;
                    angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), -Vector3.forward);
                    if (angle>0) {
                        Debug.Log("intersect");
                        agent = funnel2;
                        index1 = index2;
                        Edge newEdge = edges[index1];
                        funnel1 = (newEdge.Point2==funnel2)?newEdge.Point1:newEdge.Point2;
                        lst.Add(agent);
                    } else{
                        funnel1 = newFunnel;
                        index1 = newIndex;
                    }
                } else {
                    index1 = newIndex;
                    if (end2) {
                        Debug.Log("end");
                        agent = funnel1;
                        index2 = index1;
                        Edge newEdge = edges[index1];
                        funnel2 = (newEdge.Point2==funnel1)?newEdge.Point1:newEdge.Point2;
                        lst.Add(agent);
                    }
                }
                if (index1==edges.Count-1) {
                    end1 = true;
                }
            }

            if (!end2) {
                int newIndex = index2;
                Vector2 newFunnel = funnel2;
                NextFunnel(ref newIndex, ref newFunnel, edges, triangles);
                v1 = funnel2 - agent;
                v2 = newFunnel - agent;
                //Debug.Log(newIndex);
                angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), -Vector3.forward);
                if ((angle>0)!=false) {
                    v1 = funnel1 - agent;
                    v2 = newFunnel - agent;
                    angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), -Vector3.forward);
                    if (angle>0) {
                        Debug.Log("intersect");
                        agent = funnel1;
                        index2 = index1;
                        Edge newEdge = edges[index1];
                        funnel2 = (newEdge.Point2==funnel1)?newEdge.Point1:newEdge.Point2;
                        lst.Add(agent);
                    } else{
                        Debug.Log("Update funnel2");
                        funnel2 = newFunnel;
                        index2 = newIndex;
                    }
                } else {
                    index2 = newIndex;
                    if (end1) {
                        Debug.Log("end");
                        agent = funnel2;
                        index1 = index2;
                        Edge newEdge = edges[index1];
                        funnel1 = (newEdge.Point2==funnel2)?newEdge.Point1:newEdge.Point2;
                    lst.Add(agent);
                    }
                }
                if (index2==edges.Count-1) {
                    end2 = true;
                }
            }
            Debug.Log(index2);
            DebugShapes.DrawSphere(new Vector3(funnel1.x, funnel1.y, 0), 0.3f, Color.cyan);
            DebugShapes.DrawSphere(new Vector3(funnel2.x, funnel2.y, 0), 0.3f, Color.green);
            n++;
        }
        return lst;
    }

    private static void Traverse(ref int index, ref Vector2 funnel, List<Edge> edges, List<Triangle> triangles, Vector2 agent, bool clockwise) {
        int newIndex = index;
        Vector2 newFunnel = funnel;
        NextFunnel(ref newIndex, ref newFunnel, edges, triangles);
        Vector2 v1 = funnel - agent;
        Vector2 v2 = newFunnel - agent;
        //Debug.Log(newIndex);
        float angle = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(v2.x, v2.y, 0), -Vector3.forward);
        if ((angle>0)!=clockwise) {
            funnel = newFunnel;
            index = newIndex;
        }
    }
    */

    private static void NextFunnel(ref int index, ref Vector2 funnel, List<Edge> edges, List<Triangle> triangles) {

        while (index < triangles.Count)
        {
            Triangle currentTrig = triangles[index];
            foreach (Edge edge in currentTrig.Edges)
            {
                if (!edges.Contains(edge))
                {
                    if (edge.Point1 == funnel)
                    {
                        index++;
                        funnel = edge.Point2;
                        return;
                    }
                    else if (edge.Point2 == funnel)
                    {
                        index++;
                        funnel = edge.Point1;
                        return;
                    }
                }
            }
            index++;
        }
    }
}
