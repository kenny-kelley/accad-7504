# No School, No Peace
Our remote repository location: https://github.com/kenny-kelley/accad-7504.

## To get started...
1. Download Unity Editor Version 2019.1.9f from https://unity3d.com/get-unity/download/archive.
2. Clone our repository using:
`$ git clone https://github.com/kenny-kelley/accad-7504`
3. Open the Unity project within the Unity Editor.

## General Code Overview
We make use of "Box Triggers". These are Box GameObjects which do not render but do listen for collisions. In laymans terms, they are invisible spaces that run a script when they are entered by a player or NPC.

You will notice 13 scripts attached to these Box Triggers. Each one of these scripts is numbered and attached to a unique Box Trigger.

You will also find scripts for the characters Abdoul, Amina, Soldier 1, and Soldier 2, as well as another script to control the movement of the doors. An API of the characters and an outline of each Box Trigger is provided below.

## API
Please open this in a text editor, as it has indentations that Github doesn't show on the browser.

### AminaScript
Amina is the teacher of the class. She plays a role throughout the entire experience.

`AskIssoufWhereAreYou`			Plays the "Where are you?" audio.
`WalktoCloset`					Activates Amina's initial movement into the closet looking for Mariam.
`StopWalktoCloset`				Deactivates Amina's movement into the closet and plays the "Come along now" audio.
`WalkToFrontCenter`				Activates Amina's movement to the front center of the classroom.
`StopWalkToFrontCenter`			Deactives Amina's movement to the front center of the classroom and has her stand still.
`TakeAttendance`				Plays the taking attendance audio.
`MmphAbdoul`					Plays the angry gesture animation.
`MmphAbdoulAndIssouf`			Plays the angry gesture animation and plays the "Mariam. Abdoul." audio.
`OrderIssoufToGetChalk`			Plays the angry gesture animation and plays the "go get chalk" audio.
`TellChildrenToHide`			Plays the "Children, hide" audio.
`TellChildrenTheSoldiersAreBack`Plays the "The soldiers are back" audio.
`FaceSoldiersAndSlideBack`		Plays the terrified animation and walks backwards as she is approached.
`StopSlideBack'					Deactivates the backward movement.
`NoChalk`						Repeatedly tells Mariam to get the chalk.

### AbdoulScript
Abdoul is the friend of Mariam who tells her the story in the second phase of the School scene.

`TellStory`						Plays the talking animation and the Yasmine story audio.
`Idle`							Plays the idle animation.
`FinishStory`					Plays the remaining bit of the Yasmine story audio.
`Apologize`						Plays the talking animation and plays the apology audio.
`AskWhatsGoingOn`				Plays the "What's going on?" audio.

### Soldier1Script
Soldier 1 is the first soldier to walk into the scene. He points his weapon at Amina, the teacher.

`EnterSchool`					Activates Soldier 1's movement into the school and plays the walking animation.
`StopEnterSchool`				Deactivates Soldier 1's movement in the school and plays the "Come with us" audio.
`SayBurnThePlace`				Plays the "Burn it" audio.

### Soldier2Script
Soldier 2 is the second soldier to walk into the scene. He walks down the hall and points his weapon at Abdoul, the friend of Mariam.

`EnterSchool`					Activates Soldier 2's movement into the school and plays the walking animation.
`StopEnterSchool`				Deactivates Soldier 2's movement into the school.
`TurnAndWalkTowardCloset`		Activates Soldier 2's movement down the aisle.
`StopTurnAndWalkTowardCloset`	Deactivates Soldier 2's movement down the aisle.
`TellIssoufToMoveIt`			Plays the "Move it" audio.

### NPCScript
This is a general script for the students and is also applied to Abdoul. It provides the methods for them to exit the building.

`WalkToAisle`					Activates character movement towards the aisle and plays the walking animation.
`WalkToDoor`					Activates character movement towards the door.
`ExitSchool`					Activates character movement out of the main door of the school.

### DoorScript
This provides the methods to open/close doors.

`Close`							Closes the door it's attached to.
`Open`							Opens the door it's attached to.

## Box Trigger Outline

### Trigger 1
When the player walks in front of the mirror, Amina asks "Where are you?".

### Trigger 2
When the player approaches the closet door, Amina walks towards the door.

### Trigger 3
When Amina reaches the closet, she stops and says to come along. She then turns around and walks back into the classroom.

### Trigger 4
When Amina reaches the closest desk to the front, she angles herself more towards the front of the classroom and walks there.

### Trigger 5
When Amina reaches the front, she stops, and a timer begins.
The screen fades out after 13 seconds. Half the students are removed. The player is relocated to a desk.
The screen fades in after 15 seconds. Amina takes attendance.
Abdoul tells the story after 31 seconds.
Amina interrupts Abdoul after 44 seconds.
Abdoul finishes telling the story after 45 seconds.
Amina interrupts Abdoul and Mariam again after 55 seconds.
Abdoul apologizes after 57 seconds.
Amina orders Mariam to get chalk at 59 seconds, and repeatedly orders her if she doesn't after 20 seconds.

### Trigger 6
When the player reaches the closet to get chalk, the motorcycle audio plays. A timer begins.
Amina tells the children to hide and the village background audio stops after 10.5 seconds.
Abdoul asks "What's going on" after 14 seconds.
Amina tells the children "The soldiers are back" and the soldiers enter the school after 15.5 seconds.

### Trigger 7
When Amina slides back towards the desk, she stops.

### Trigger 8
When Soldier 1 is adequately in the building, he stops.

### Trigger 9
When Soldier 2 is adequately in the building, he stops.
The four NPCs begin walking out.
Soldier 2 begins walking toward the closet.

### Trigger 10
When Soldier 2 makes it down the aisle, he tells Abdoul to move it.

### Trigger 11
There are four Box Triggers that have this script attached to them. Each of them are placed in the aisle to begin the students movement towards the front.
When the students (including Abdoul) reach these, they begin their movement to the front and out of the classroom.

### Trigger 12
When the students leave the classroom, it updates a field noting that they have made it out of the building.

### Trigger 13
When all of the students have left the classroom, Soldier 1 says to "Burn it" and fire begins to appear.
The scene comes to an end.