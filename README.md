![Badge](https://img.shields.io/badge/.NET%20Framework-4.6.1-information??style=for-the-badge&logo=.NET&logoColor=white&color=512BD4)  ![AUR license badge](https://img.shields.io/badge/Apache-2.0-blue???style=for-the-badge&logo=apache)  ![Badge](https://img.shields.io/badge/Visual%20Studio-2022-information??style=for-the-badge&logo=VisualStudio&logoColor=white&color=512BD4)  
# Waves Projector

A WPF UI Element that reproduces animations based on sprite sheet files.

## Content

<!--ts-->
*  [License](#License)
*  [The Projector](#the-projector)
* * [Usage](#usage)
* * [Properties](#properties)
* [Release Notes](#release-notes)
<!--te-->

## License
This project is licensed under the terms of [Apache 2.0 license](https://github.com/WAVES-Systems/Projector/blob/main/LICENSE).

## The Projector
[ You can check the full documentation in [wavessystems.com.br/projector](https://wavessystems.com.br/projector). ]

I worked to make it as familiar as possible with other WPF controls, so using this control is quite simple! Check the following steps:

### Usage
First thing, define the namespace in the parent object attributes.
```xml
xmlns:visual="clr-namespace:Waves.Visual;assembly=Waves.Visual.Projector"
```

Right! Now you can place the element like this:
```xml
<visual:Projector FrameCount="293"
                  ColumnCount="15"
                  Source="/Assets/nooooooo.png"
                  Width="800"
                  Height="450" />
```
You got define the URI of the sprite sheet and then define the number of frames and the number of columns. The number of rows, let me automatically calculate for you. 

**Please do not forget**: the frame and column count are especially important to run the animation properly. Mistaking these property values will create a visual mess.

I have not implemented the Width and Height automatic calculation for when these properties are set as "Auto" just like when you set source of an [Image](https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.image?view=windowsdesktop-7.0) object, but it is on the plan.

### Properties
This control implements some properties from [Timeline](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.animation.timeline?view=windowsdesktop-7.0), so we have basic animation properties such as [FillBehavior](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_FillBehavior.htm), [RepeatBehavior](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_RepeatBehavior.htm) and [AutoReverse](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_AutoReverse.htm), as shown in the following table:

| Property                                                                                                 | Description                                                                                                        |
|----------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------|
| [AutoReverse](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_AutoReverse.htm)       | Gets or sets a value that indicates whether the animation plays in reverse after it completes a forward iteration. |
| [AutoStart](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_AutoStart.htm)           | Gets of sets a value that indicates whether the animation starts automatically after loading.                      |
| [ColumnCount](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_ColumnCount.htm)       | Gets or Sets the amount of columns in the sprite animation sheet.                                                  |
| [Duration](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_Duration.htm)             | Gets the length of time for which this Animation plays, not counting repetitions.                                  |
| [FillBehavior](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_FillBehavior.htm)     | Gets or sets a value that specifies how the animation behaves after it reaches the end of its active period.       |
| [FrameCount](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_FrameCount.htm)         | Gets or Sets the amount of frames in the sprite animation file.                                                    |
| [FrameRate](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_FrameRate.htm)           | Gets or Sets the frame per second rate of animation.                                                               |
| [IsPlaying](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_IsPlaying.htm)           | Gets or sets a value that indicates whether the animation is running.                                              |
| [RepeatBehavior](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_RepeatBehavior.htm) | Gets or sets the repeating behavior of this animation.                                                             |
| [Source](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_Source.htm)                 | Gets or Sets the animated image displayed in the view.                                                             |
| [TotalDuration](https://wavessystems.com.br/projector/html/P_Waves_Visual_Projector_TotalDuration.htm)   | Gets the length of time for which this Animation plays, counting repetitions.                                      |

### Methods

| Method              | Description                                                                                              |
|---------------------|----------------------------------------------------------------------------------------------------------|
| [BeginAnimation()](https://wavessystems.com.br/projector/html/M_Waves_Visual_Projector_BeginAnimation.htm)    | Starts or resumes the animation.                                                                         |
| [BeginAnimationAsync()](https://wavessystems.com.br/projector/html/M_Waves_Visual_Projector_BeginAnimationAsync.htm) | Starts or resumes the animation and holds the execution until the animation completes its active period. |
| [Dispose()](https://wavessystems.com.br/projector/html/M_Waves_Visual_Projector_Dispose.htm)       | Releases all resources used by the [Projector](https://wavessystems.com.br/projector/html/T_Waves_Visual_Projector.htm)                                                                                    |
| [StopAnimation()](https://wavessystems.com.br/projector/html/M_Waves_Visual_Projector_StopAnimation.htm)       | Stops the animation.                                                                                   

### Events
| Event     | Description                                                                                           |
|-----------|-------------------------------------------------------------------------------------------------------|
| [Completed](https://wavessystems.com.br/projector/html/E_Waves_Visual_Projector_Completed.htm) | Occurs when this animation has completely finished playing and will no longer enter its active period. |

## Release Notes
v3.0.2.0:  
* Logical improvements;
* Added lag frame skipping;
