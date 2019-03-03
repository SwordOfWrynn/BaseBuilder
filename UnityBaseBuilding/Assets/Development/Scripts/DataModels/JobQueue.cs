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
        if(_j.jobTime < 0)
        {
            //job has a negative time, so instantly complete it

            _j.DoWork(0);
            return;
        }

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

    public void Remove(Job _j)
    {
        List<Job> jobs = new List<Job>(jobQueue);

        if(jobs.Contains(_j) == false)
        {
            Debug.LogError("Trying to remove a job that is not on the queue");
            return;
        }

        jobs.Remove(_j);
        jobQueue = new Queue<Job>(jobs);
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
