![Badge](https://img.shields.io/badge/.NET%20Framework-4.6.1-information??style=for-the-badge&logo=.NET&logoColor=white&color=512BD4)  ![AUR license badge](https://img.shields.io/badge/license-Apache-blue???style=for-the-badge&logo=apache)  ![Badge](https://img.shields.io/badge/Visual%20Studio-2022-information??style=for-the-badge&logo=VisualStudio&logoColor=white&color=512BD4)  
# Waves Projector

WPF UI Element that reproduces animations based on sprite sheet files.

## Content

<!--ts-->
*  [License](#License)
*  [The Projector](#the-projector)
* * [Properties](#properties)
* * [Usage](#usage)
* [Release Notes](#release-notes)
<!--te-->

## License
This project is licensed under the terms of [Apache 2.0 license](https://github.com/WAVES-Systems/Projector/blob/main/LICENSE.txt).

## The Projector

I worked to make it as familiar as possible with other WPF controls, such as [Timeline](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.animation.timeline?view=windowsdesktop-7.0), so we have basic animation properties such as FillBehavior, RepeatBehavior and AutoReverse, as shown in the following table:

### Properties
| Property      | Description                                                                                                         |
|---------------|---------------------------------------------------------------------------------------------------------------------|
| AutoReverse   | Gets or sets a value that indicates whether the animation plays in reverse after it completes a forward iteration.  |
| AutoStart     | Gets of sets a value that indicates whether the animation starts automatically after loading.                       |
| ColumnCount   | Gets or Sets the amount of columns in the sprite animation sheet.                                                   |
| Duration      | Gets the length of time for which this Animation plays, not counting repetitions.                                   |
| FillBehavior  | Gets or sets a value that specifies how the animation behaves after it reaches the end of its active period.        |
| FrameCount    | Gets or Sets the amount of frames in the sprite animation file.                                                     |
| FrameRate     | Gets or Sets the frame per second rate of animation.                                                                |
| IsPlaying     | Gets or sets a value that indicates whether the animation is running.                                               |
| Source        | Gets or Sets the animated image displayed in the view.                                                              |
| TotalDuration | Gets the length of time for which this Animation plays, counting repetitions.                                       |

### Usage
Using this control is quite simple. Check the following steps:

First thing, define the namespace in the parent object attributes.
```xml
xmlns:visual="clr-namespace:Waves.Visual;assembly=Waves.Visual.Projector"
```

Right! Now you can place the element like this:
```xml
<visual:Projector FrameCount="293"
                  ColumnCount="15"
                  RepeatBehavior="Forever"
                  FillBehavior="Stop"
                  Source="/Assets/nooooooo.png"
                  Width="800"
                  Height="450" />
```
You got define the URI of the sprite sheet and then define the number of frames and the number of columns. The number rows, let me automatically calculate for you. 

**Please do not forget**: the frame and column count are especially important to run the animation properly. Mistaking these property values will create a visual mess.

I have not implemented the Width and Height automatic calculation for when these properties are set as "Auto" just like when you set source of an [Image](https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.image?view=windowsdesktop-7.0) object, but it is on the plan.

### Methods

| Method              | Description                                                                                              |
|---------------------|----------------------------------------------------------------------------------------------------------|
| BeginAnimation()    | Starts or resumes the animation.                                                                         |
| BeginAnimationAsync() | Starts or resumes the animation and holds the execution until the animation completes its active period. |
| StopAnimation()       | Stops the animation.                                                                                     |

### Events
| Event     | Description                                                                                           |
|-----------|-------------------------------------------------------------------------------------------------------|
| Completed | Occurs when this animation has completely finished playing and will no longer enter its active period. |

You can check the full documentation in [wavessystems.com.br/projector](https://wavessystems.com.br/projector).

## Release Notes
v3.0.1.0:  
* FillBehavior property;  
* RepeatBehavior property with full implementation;  
* AutoReverse property;  
* Completed event;  
* BeginAnimationAsync method;
* Fix image going blank when setting dimension properties to "Auto";