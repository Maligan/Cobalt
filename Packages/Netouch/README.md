# Netouch

.NET/Unity multitouch gesture library inspired by [Apple UIKit](https://developer.apple.com/documentation/uikit/uigesturerecognizer) & [fljot/Gestouch](https://github.com/fljot/Gestouch)

## Installation

* Install via UPM (Unity Package Manager) 
	- Navigate to `Window/Package Manger`
	- Press plus button (top-left corner)
	- Select `Add package from git URL...`
	- Enter repo url `git@github.com:Maligan/unity-netouch.git`
* Install manually
	- Copy repo content (or add as submodule if you brave enough) into your `$PROJECT/Packages` folder

## Initialize

```
import Netouch;
import Netouch.Unity;

void Awake() // or Start()
{
	Gesture.dpi = Screen.dpi;
	Gesture.Add(new UnityHitTester());
	Gesture.Add(new UnityTouchInput());
}
```

## Gestures

* TapGesture
* SwipeGesture
* PanGesture
* LongPressGesture

## Samples

```
// All gesture can accept optional target GameObject, if target not specified gesture will be screen-wide
var target = GameObject.Find("Image");

// Instantiate gestures
var singleTap = new TapGesture(target);
var doubleTap = new TapGesture() { NumTapsRequired = 2 };

// Require-to-fail dependency, without this signle tap will be recognized twice while double tapping 
single.Require(doubleTap);

// Subscribe
signle.On(GestureState.Recognized, x => Debug.Log("Single tap on Image"));
double.On(GestureState.Recognized, x => Debug.Log("Double tap on screen"));
```

## Under the hood

Any gesture can be discrete (like Tap, Swipe, etc) or continious (like Pinch, Zoom, Drag, etc). Each type can be in different state at the moment:

```
[None] -> [Possible] -> [Recognized]
					 -> [Began] -> [Update] -> [End]
					 -> [Failed]
```


