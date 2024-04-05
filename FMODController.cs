using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;
using Unity.Mathematics;
using System.Linq;
using STOP_MODE = FMOD.Studio.STOP_MODE;

[Serializable]
public class FMODEventInstanceWrapper
{
    public string eventName;
    public EventInstance eventInstance;
    public EventDescription eventDescription;
    public List<FMODParameterWrapper> parameters = new();


    public void SetParameter(string _param, float value)
    {
        FMODParameterWrapper parameter = parameters.Find(_ => _.paramName == _param);
        SetParameter(parameter, value);
    }
    public void SetParameterUnscaled(string _param, float value)
    {
        FMODParameterWrapper parameter = parameters.Find(_ => _.paramName == _param);
        SetParameterUnscaled(parameter, value);
    }


    public void SetParameter(FMODParameterWrapper parameter, float value)
    {
        value = math.remap(0f, 1f, parameter.minimum, parameter.maximum, value);
        eventInstance.setParameterByID(parameter.id, value);
    }

    public void SetParameterUnscaled(FMODParameterWrapper parameter, float value)
    {
        eventInstance.setParameterByID(parameter.id, value);
    }
}

[Serializable]
public class FMODParameterWrapper
{
    public string paramName;
    public float minimum;
    public float maximum;
    public PARAMETER_DESCRIPTION description;
    public PARAMETER_ID id;

}

public class FMODController : MonoBehaviour
{


    public static FMODController instance;

    public List<FMODEventInstanceWrapper> events = new();

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        // Load already existing events
        Bank[] banks;
        RuntimeManager.StudioSystem.getBankList(out banks);

        foreach (Bank b in banks)
        {
            EventDescription[] descriptions;
            b.getEventList(out descriptions);

            foreach (EventDescription desc in descriptions)
            {
                EventInstance[] instances;
                desc.getInstanceList(out instances);

                // Handle instance data
                foreach (EventInstance inst in instances)
                {
                    events.Add(CreateWrapperFromInstance(inst));
                }

            }

        }
    }

    public EventInstance PlayEvent(string eventReferenceName)
    {
        //print($"Will try to play {eventReferenceName}");
        //StartCoroutine(PlayEventCoroutine(eventReferenceName));
        return PlayEventCoroutine(eventReferenceName);
    }

    /*
    EventInstance PlayEventCoroutine(string eventReferenceName)
    {
        RuntimeManager.CreateInstance($"event:/{eventReferenceName}");

        return 
    }
    */

    FMODEventInstanceWrapper CreateWrapperFromInstance(EventInstance instance)
    {
        FMODEventInstanceWrapper _event = new();

        // Load the event details
        //_event.eventInstance = RuntimeManager.CreateInstance(eventReference); ============================
        _event.eventInstance = instance;

        //_event.eventName = eventReference.Path; ============
        EventDescription desc;
        instance.getDescription(out desc);
        string pathName;
        desc.getPath(out pathName);

        // Filter out "event:/" from pathname (just crop it
        _event.eventName = pathName.Substring(7);

        // Set 3D attributes
        _event.eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));

        _event.eventInstance.getDescription(out _event.eventDescription);

        // Iterate through the event and retrieve the parameters
        _event.eventDescription.loadSampleData();

        // Add parameters
        int parameterCount;
        _event.eventDescription.getParameterDescriptionCount(out parameterCount);
        //print(parameterCount);
        for (int i = 0; i < parameterCount; i++)
        {
            PARAMETER_DESCRIPTION parameterDescription;
            _event.eventDescription.getParameterDescriptionByIndex(i, out parameterDescription);
            //print((string) parameterDescription.name);

            FMODParameterWrapper FMPW = new();
            FMPW.paramName = (string)parameterDescription.name;
            FMPW.description = parameterDescription;
            FMPW.id = parameterDescription.id;
            FMPW.minimum = parameterDescription.minimum;
            FMPW.maximum = parameterDescription.maximum;
            _event.parameters.Add(FMPW);
        }




        return _event;
    }

    EventInstance PlayEventCoroutine(string eventReferenceName)
    {
        return PlayEventCoroutine(RuntimeManager.CreateInstance($"event:/{eventReferenceName}"));
    }

    // IEnumerator
    EventInstance PlayEventCoroutine(EventInstance instance)
    {

        //if (!FMODUnity.EventManager.IsInitialized) FMODUnity.EventManager.Startup();

        /*
        // Wait for data to load on timer
        float timeout = 1f;
        bool hasLoaded = false;
        for (float i = 0; i < timeout; i += Time.deltaTime)
        {
            LOADING_STATE state;
            eventDescription.getSampleLoadingState(out state);

            if (state == LOADING_STATE.LOADED)
            {
                hasLoaded = true;
                break;
            }
            yield return null;
        }
        if (!hasLoaded)
        {
            print("Took too long to load!");
            yield break;
        }
        */

        FMODEventInstanceWrapper _event = CreateWrapperFromInstance(instance);
        // Start the instance once data is loaded
        _event.eventInstance.start();
        events.Add(_event);

        //print(_event.eventName + " has been played (?)");
        /*
        PARAMETER_DESCRIPTION nextSectionParameterDescription;
        nextSectionDescription.getParameterDescriptionByName("Voice", out nextSectionParameterDescription);
        sectionParameterID = nextSectionParameterDescription.id;
        */

        //print(sectionParameterID);

        return _event.eventInstance;

    }

    public void StopEvent(EventInstance eventInstance, bool immediateStop = true)
    {
        StopEvent(FindEvent(eventInstance), immediateStop);
    }

    public void StopEvent(string eventReferenceName, bool immediateStop = true)
    {
        StopEvent(FindEvent(eventReferenceName), immediateStop);
    }

    void StopEvent(FMODEventInstanceWrapper eventToStop, bool immediateStop = true)
    {
        if (events.Count > 0)
        {
            // Get the specified event and start it
            eventToStop.eventInstance.stop(immediateStop ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
            eventToStop.eventInstance.release();
            events.Remove(eventToStop);
        }
    }

    public FMODEventInstanceWrapper FindEvent(string eventName)
    {
        var result = events.Find(_ => _.eventName == "event:/" + eventName);
        if (result != null)
        {
            return result;
        }

        return null;

        /*
        if (addIfNotExist)
        {
            return PlayEvent(eventName);
        }
        else
        {
            return null;
        }
        */
    }
    public FMODEventInstanceWrapper FindEvent(EventInstance eventInstance)
    {
        return events.Find(_ => _.eventInstance.Equals(eventInstance));
    }

    public int FindEventIndex(string eventName)
    {
        int eventIndex = events.FindIndex(_ => _.eventName == "event:/" + eventName);
        return eventIndex;
    }

    public EventInstance FindEventInstance(string eventName)
    {
        print("Looking for " + eventName);
        foreach (var event_ in events)
        {
            print(event_.eventName);
            print(event_.eventName == eventName);

        }

        return events.Find(_ => _.eventName == eventName).eventInstance;
    }

    public void CheckForFinishedEvents()
    {
        foreach (var FMODEvent in events.ToList())
        {
            PLAYBACK_STATE state;
            FMODEvent.eventInstance.getPlaybackState(out state);

            if (state == PLAYBACK_STATE.STOPPED)
            {
                FMODEvent.eventInstance.release();
                events.Remove(FMODEvent);
            }
        }
    }

    private void Update()
    {
        //eventInstance.setParameterByID(sectionParameterID, voice)
        CheckForFinishedEvents();
    }

}
