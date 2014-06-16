Timeliner
=========
A [Posh] (https://github.com/vvvv/Posh) based timeline that can be controlled by and sends out its values via [OSC] (http://opensoundcontrol.org). 

Brought to you by [vvvv] (http://vvvv.org).

Requires Internet Explorer >= 10 to be installed on your system.

### Track Types
* Value
* String

### Mouse Interaction
* create a track via doubleclick
* create a keyframe in a track via doubleclick
* show track menu via middleclick on a track
* show keyframe menu via middleclick on a keyframe
* add keyframes to selection by pressing Ctrl while selecting
* remove keyframes from selection by pressing Alt while selecting
* in menues:
 * change numbers using the mouse wheel (also use Shift/Ctrl/Alt vvvv-style to change stepsize)
 * change numbers and text via rightclick
* pan (scroll in time) via right-drag left/right
* zoom via right-drag up/down
* rearrange tracks via left dragging their labels 

### Keyboard Interaction
Function| Shortcut
------------- | -------------
Toggle Play | SPACE
Stop | BackSpace
Undo | Ctrl + Z
Redo | Ctrl + Shift + Z
Set in point | I
Set out point | O
Select all keyframes in active track | Ctrl + A
Select all keyframes | Ctrl + Shift + A
Delete selected keyframes | Del
Toggle Collapse active track | Ctrl+<
Nudge selected keyframes by one frame | Left/Right Arrow Keys
Nudge selected keyframes values | Up/Down Arrow Keys (also use Shift/Ctrl/Alt vvvv-style to change stepsize)

### Receiving OSC
All of Timeliners current values are being sent via UDP using the OSC protocoll. Specify a target IP address (default: 127.0.0.1 ie. localhost) and a port (default: 4444) via Main Menu -> OSC.

All values are sent in one OSC-Bundle. The individual messages addresses comprise of the specified "Prefix" + the pinname. e.g.:
* /timeliner/Value 0
* /timeliner/String 0

### Sending OSC
TimelinerSA can be remote controlled via OSC commands. It listenes to commands sent to the port set via the "Receive Port" numberbox in the interface (which defaults to 5555).

Send the commands to the "Prefix" + the command you like to controll. e.g.:

play: takes 0 (pause) or 1 (play) as argument

/timeliner/play 1

stop: no arguments

/timeliner/stop

seek: takes a single floating point value to specify the time to seek to

/timeliner/seek 0.234

loop: takes two floating point values to specify the loops in and out points

/timeliner/loop 0.456 1.234

### Web Access

Navigate to http://127.0.0.1:4444 with your browser to see a list of available timelines.

If you want to access a timeline other than via localhost make sure to run TimelinerSA.exe as admin.

---

For similar projects see:
* [TimelinerSA] (http://vvvv.org/documentation/timelinersa)
* [Duration] (http://www.duration.cc/)
* [Vezér] (http://www.vezerapp.hu/)
* [KluppeTimeLine] (http://core.servus.at/node/1424)
* [IanniX] (http://www.iannix.org/)
