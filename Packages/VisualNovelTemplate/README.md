# Unity-VisualNovel-Template
Opinionated visual novel framework built in Unity. Lots of dependencies but all (mostly) free!

**NOTE: We are using Odin Inspector which is a PAID plugin. In the future, we will remove this paid dependency (either by replacing it with a free solution/custom editor scripting, or a conditional checking for the package being installed). As of July 1, 2022, you MUST have Odin Inspector installed to import this package!!!**

The only non-free package is Odin Inspector, which is unnecessary for the package functions.

This framework is powered by [`Ink`](https://openupm.com/packages/com.inklestudios.ink-unity-integration/), a powerful, free narrative tool that includes its own save system
and has C# Unity integration. 

We're providing a UI system and some convenience functions/features that we use in our visual novels. Most of these features come out of the box with Ren'Py.

# Requirements
To install this package, first install all requirements (OpenUPM, UNity package manager, and Additional). Then install this package.

### OpenUPM
Installed through OpenUPM (using relative paths in the dependency in `package.json`)

```openupm add com.neuecc.unirx com.cysharp.unitask com.demigiant.dotween com.inklestudios.ink-unity-integration```

* com.neuecc.unirx (UniRx)
* com.cysharp.unitask (UniTask)
* com.demigiant.dotween (DOTween)
* com.inklestudios.ink-unity-integration (Ink)

### Unity package manager

* Addressables
* New InputSystem
### Additional
Free but requires external installation

* FMOD

### PAID

* Odin Inspector
# Features
Out of the box, you get...

* NVL and ADV textboxes
* Paper doll ready sprite control with fade transitions between expressions/poses
## Menus

* Main Menu, Main Menu Settings, Extras (CG Gallery, Music Gallery), Credits, About
* In-game Settings

## Misc
* Video player

## Accessibility

* [] OpenDyslexic font
* Closed captions
* [] Variable font size 

## Google Play

* Advertisement plugin integration example
# How to use this package

# ROADMAP
- [] Text to speech (UAP plugin?)
