using System;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {

    Queue<Job> jobQueue;

    Action<Job> cbJobCreated;

    public JobQueue()
    {
        jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job _j)
    {
        jobQueue.Enqueue(_j);

        if(cbJobCreated != null){
            cbJobCreated(_j);
        }
    }

    public Job Dequeue()
    {
        if (jobQueue.Count == 0)
            return null;

        return jobQueue.Dequeue();
    }


    public void RegisterJobCreationCallback(Action<Job> _cb)
    {
        cbJobCreated += _cb;
    }

    public void UnregisterJobCreationCallback(Action<Job> _cb)
    {
        cbJobCreated -= _cb;
    }

}
