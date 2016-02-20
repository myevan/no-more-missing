# NoMoreMissing
 
NoMoreMissing fixes missing scripts in unity project.

## Note

**MUST BACKUP** your project before using thie plugin that marks a behaviour **CLASS NAME** in **ALL** prefabs. 

## Use Cases

### Managed Behaviour

* Create prefabs with module sources.

* Compile module sources and make a library

    iTween.cs -> iTween.dll

* **Click the "Help/Prepare for Missing scripts" menu.**

* Remove module sources and copy the library

    $ rm iTween.cs 
    $ copy iTween.dll Assets/iTween.dll

* **Click the "Help/Fix Missing scripts" menu.**

* Restart your unity.


### Missing Behaviour meta 

* Make a game object

    Hierarchy:

        example

* Make a behaviour

    Project:
        
        Assets/
            TestBehaviour.cs

* Add the behaviour to the game object 

    Inspector:

        Transform:
            ...

        TestBehaviour:
            Name: john
            Score: 75 

* Save a prefab for the game object

    Project:
        
        Assets/
            example.prefab

* **Click the "Help/Prepare for Missing scripts" menu.**

* Save the scene and quit your unity.

* Remove the behaviour meta file and the Library folder for making a broken prefab (includes **MISSING SCRIPT**)

        $ rm Assets/TestBehaviour.cs
        $ rm Library

* Open the scene and check a **MISSING SCRIPT** of example object

* **Click the "Help/Fix Missing scripts" menu.**

* Close and re-open the scene and re-check a **FIXED SCRIPT** of example object
