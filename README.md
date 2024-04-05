# Simple Unity FMOD Wrapper
 Simple Wrapper focused on basic playback and parameter manipulation.

## Usage
You must have FMOD for Unity installed in your unity project.
___

### Classes
#### FMODController
The main class. Plays, stops, and manipulates sounds.
#### FMODEventInstanceWrapper
A wrapper for a currently playing sound (created automatically). Use this class to change parameters.
#### FMODParameterWrapper
A wrapper for FMOD parameters (created automatically).

___

### Functions

#### FMODController.instance.PlayEvent(string eventReferenceName)
Plays event matching the name eventReferenceName.
*Returns EventInstance.*

#### FMODController.instance.StopEvent(EventInstance eventInstance, bool immediateStop = true)<br>FMODController.instance.StopEvent(string eventReferenceName, bool immediateStop = true)
Stops playback of the eventInstance.
If given the eventInstance name, stops playback of the first found instance matching it.

#### FMODController.instance.FindEvent(EventInstance eventInstance)<br>FMODController.instance.FindEvent(string eventName)
Returns the first instance matching the given eventName / eventInstance.
*Returns FMODEventInstanceWrapper.*

#### FMODController.instance.FindEventIndex(string eventName)
Returns the index of the matching eventInstance that matches the name eventName.
