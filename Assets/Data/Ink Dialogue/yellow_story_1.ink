INCLUDE functions.ink

=== start ===
Right then, adventurer! Fancy a few tasks? They're <b>vital</b>, you see. Absolutely critical.
+   p: Of course! Let the vital tasks commence!
    ~ set_quest_status("yellow_quest", "Active")
    -> task_1
+   p: I sense sarcasm. No way!
    -> quest_failed

=== quest_failed ===
~ set_quest_status("yellow_quest", "Failed")
-> loop

= loop
Out of my sight, wretched one!
+   p: If you say so...
    >>> close
    -> loop


=== task_1 ===
Your first task, a classic trial!
Slay 100 wolves and bring their pelts directly to me, {get_name(npc_id)}.
The fate of... something... hangs in the balance! <b>Hurry, hurry!</b>
+   p: A worthy quest, indeed! I shall slay the beasts immediately.
    ~ set_task_status("yellow_quest", "wolf_pelts", "Active")
    >>> close
    -> task_2
+   p: 100 wolves? That sounds tedious. I think not!
    -> quest_failed


=== task_2 ===
Ah, you're back! Excellent work with those wolves.
Truly <i>ferocious</i> beasts, I imagine.
Did you bring the pelts?

+   {count_item(player_id, "Wolf Pelt") >= 100} p: Of course I did!
    ~ set_task_status("yellow_quest", "wolf_pelts", "Completed")
    ~ remove_item(player_id, "Wolf Pelt", 100)
    <i>Doesn't matter!</i>
    I've forgotten why I needed them. Probably wasn't important.
    -> continue_task_2
+   {count_item(player_id, "Wolf Pelt") < 100} p: Uhhh...
    What impudence!
    I absolutely need these 100 pelts. For the fate of... something...
    ++  p: Then I shall redouble my efforts!
        >>> close
        -> task_2

= continue_task_2
Anyway, on to the second task:
My ceremonial pet rock, Dwayne, needs his armour polished.
It's over there. Use this special cloth.
Don't ask where the armour came from.
+   p: Polish Dwayne's armour? Right away!
    ~ set_task_status("yellow_quest", "polish_armor", "Active")
    >>> close
    -> finish_task_2

+   p: I slay wolves, I don't polish rocks. Even armoured ones.
    -> quest_failed

= finish_task_2
~ set_task_status("yellow_quest", "polish_armor", "Completed")
~ temp old_currency = get_currency(player_id)
~ add_currency(player_id, 5)
Excellent! Dwayne looks positively gleaming! Here's 5 dollars for your trouble.
You now have {get_currency(player_id)} dollars.
You previously had {old_currency} dollars.
+   p: Another worthy task done![] Generations will sing of my achievements!
    >>> close
    -> task_3


=== task_3 ===
Right, the third task! This requires finesse.
You must escort Gary the Gourmet Snail across the town square.
He's heading to {get_name(second_yellow_npc_id)}'s place for a lettuce tasting.
Gary is... deliberate. And requires hydration.
<i>Do you have a Small Health Potion?</i>

+   {count_item(player_id, "Small Health Potion") > 0} [Check Potion] p: I have a potion!
    ~ remove_item(player_id, "Small Health Potion", 1) // Gary drinks the potion

    ~ set_task_status("yellow_quest", "escort_gary", "Active")
    ~ set_task_status("yellow_quest", "talk_to_other_npc", "Active")

    Good, good! Off you go!
    ++ p: Onwards to another adventure!
        >>> close
        -> gary_drank_health_potion
+ {count_item(player_id, "Small Health Potion") == 0} [Check Potion] p: I don't have a Small Health Potion.
    Then Gary cannot undertake his perilous journey! Come back when you find one.
    ++ p: I shall endeavor to find one.
       >>> close
       -> task_3
+ p: Escort a snail? Absolutely not.
    Gary looks disappointed. Or maybe that's just his normal expression.
    -> quest_failed

= gary_drank_health_potion
{
    - get_task_status("yellow_quest", "talk_to_other_npc") == "Completed":
        Wonderful! Gary appreciated the hydration. He made it safely!
        {get_name(second_yellow_npc_id)} sends their thanks and this
        <> Medium Health Potion!

        ~ add_item(player_id, "Medium Health Potion", 1)
        ~ set_task_status("yellow_quest", "escort_gary", "Completed")

        ++ [Take the potion] p: What plentiful bounty!
           >>> close
            -> task_4
    - else:
        Hurry and bring Gary to {get_name(second_yellow_npc_id)}!
        ++ p: All right, all right...
        >>> close
        -> gary_drank_health_potion
}

=== task_4 ===
You find {get_name(first_yellow_npc_id)} looking flustered.

"Ah, the snail-escort! Good job! Now, a task of utmost importance.
I need exactly seven <b>perfectly spherical</b> pebbles.
Not oval, not mostly round, <b>perfectly spherical</b>.
For... reasons. Geological reasons.

You have {count_item(player_id, "Pickaxe")} pickaxe(s). Maybe that helps? Or hinders? Who knows."
+   p: Perfectly spherical pebbles? <i>My specialty!</i>
    Here's an Iron Sword for your... enthusiasm. Try not to hurt yourself.
    ~ add_item(player_id, "Iron Sword", 1)
    Now, off you go!
    ++ p: Off I go...
        ~ set_task_status("yellow_quest", "spherical_pebbles", "Active")
        >>> close
        -> task_4_completed
+   p: Searching for round rocks sounds dreadfully boring.
    "Suit yourself," sighs {get_name(first_yellow_npc_id)}. "Some people just don't appreciate geology."
    -> quest_failed

= task_4_completed
You return to {get_name(first_yellow_npc_id)} with the pebbles.
They seem pleased, maybe?
"Marvellous! Spherical indeed!
~ set_task_status("yellow_quest", "spherical_pebbles", "Completed")
+   p: Perfect spheres, they are!
    >>> close
    -> task_5


=== task_5 ===

"Okay, the fifth task: Deliver this package to {get_name(second_yellow_npc_id)}."

"Don't shake it. It's... fragile."
"And possibly contains bees. Or an angry badger. Or maybe just biscuits."

It's a small wooden box that seems to be faintly vibrating.
+   p: Deliver the vibrating, possibly badger-filled box? Sounds like fun!
    ~ set_task_status("yellow_quest", "deliver_vibrating_package", "Active")
    Ah, one more thing.
    There's a small delivery fee. My fee. 10 dollars. Hand it over.
    ++ p: <i>Hand over 10 dollars</i>
        ~ remove_currency(player_id, 10)
        Pleasure doing business with you! You now have {get_currency(player_id)} dollars.
        +++ p: Somehow I feel, I just got tricked...
            >>> close
            -> task_5_started
+   p: That sounds dangerous and expensive. No thank you.
    "Fine, I'll just get the snail to do it. Might take a while."
    -> quest_failed

= task_5_started
{
    - get_task_status("yellow_quest", "deliver_vibrating_package") == "Active":
        Hurry up! This thing might explode every second now!
        +   p: Uh, OK, then...
            >>> close
            -> task_5_started
    - get_task_status("yellow_quest", "deliver_vibrating_package") == "Failed":
        So you did open the package...
        How disappointing.
        +   p: Leave dejectedly
            >>> close
            -> end_quest
    - get_task_status("yellow_quest", "deliver_vibrating_package") == "Completed":
        I knew that I could count on you!
        +   p: But of course! you proudly exclaim
            >>> close
            -> end_quest
    - else:
        I don't know how we got here...
        +   p: Neither do I!
            >>> close
            -> end_quest
}

=== end_quest ===
Well done! That was everything I had for you.
Come back later.
~ set_quest_status("yellow_quest", "Completed")
+   p: I'll be back!
    >>> close
    -> nothing_to_do

= nothing_to_do
For now I have no quests for you.
+   p: I'll check back later!
    >>> close
    -> nothing_to_do

/*
=== task_6 ===
You find {get_name(second_yellow_npc_id)}, who takes the box cautiously.
"Ah, the... package. Excellent. Didn't explode. Good. Task 6 is a riddle, a test of intellect!
Behold this ancient, coded message: 'Ethay Easuretray isyay inyay ethay Endorvay'syay Ocketpay.'
What could it mean?! It has baffled scholars for minutes!"
+   [Solve Riddle] "The Treasure is in the Vendor's Pocket." Easy, it's Pig Latin.
    "Pig Latin? By the gods! You've cracked it! Incredible! I must inform the scholars!"
    Quickly, go see {get_name(vendor_id)}! Find this treasure!
    -> task_7
+   That sounds complicated. My head hurts.
    "Yes, it is a powerful enigma," {get_name(second_yellow_npc_id)} says proudly. "Run along if it's too much for you."
    -> quest_failed


=== task_7 ===
You approach {get_name(vendor_id)}, a shifty-looking individual polishing an apple.
"Looking for treasure, eh? Heard you cracked the code. Smart one.
Alright, alright, the 'treasure'. It's this magnificent Rubber Chicken. Been in the family for generations. Probably."
Can you even carry such a majestic item? Let's see...
+ {can_add_item(player_id, "Rubber Chicken", 1)} [Check Inventory Space] Yes, I have space for this... treasure.
    ~ add_item(player_id, "Rubber Chicken", 1) // Need to define "Rubber Chicken" as a unique item
    There you go. Treat Bartholomew Clucksworth III with respect.
    Here's 15 dollars for... finding him? Taking him off my hands? Whatever.
    ~ add_currency(player_id, 15)
    + + 15 whole dollars?
        >>> close
        -> task_8
+  {not can_add_item(player_id, "Rubber Chicken", 1)} [Check Inventory Space] My bags are too full for such a prize!
    "Bah! Come back when you've made room for TRUE treasure!"
    + + I will...
        >>> close
        -> task_7
+ A rubber chicken? That's the treasure? I pass.
    "Your loss! More glorious chicken for me!"
    -> quest_failed


=== task_8 ===
You return to {get_name(pink_npc_id)}, holding the rubber chicken.
"Bartholomew! Excellent! Now for Task 8: Potion Crafting!
We shall create the legendary 'Potion of Mild Convenience'! It makes things... slightly easier. Sometimes.
Ingredients: 1 Rubber Chicken (check!), and... let's say, two Medium Health Potions."
Do you have the potions? You currently have {count_item(player_id, "Medium Health Potion")}.
+ {count_item(player_id, "Medium Health Potion") >= 2} [Check Potions] I have the chicken and the potions! Let's brew!
    ~ remove_item(player_id, "Rubber Chicken", 1)
    ~ remove_item(player_id, "Medium Health Potion", 2)
    _Splendid!_ After much bubbling and a faint smell of ozone and poultry... it is done!
    Sadly, the potion is intangible and exists only conceptually. But for your effort, take this Steel Sword!
    ~ add_item(player_id, "Sword (Steel)", 1)
    ++ That sounds... like a bad deal!
       >>> close
       -> task_9
+  {count_item(player_id, "Medium Health Potion") < 2} [Check Potions] I don't have enough Medium Health Potions.
    "A setback! Go find more, then return to create... mild convenience!"
    ++ No amount of trouble is too great for... mild convenience!
       >>> close
       -> task_8
+ Brewing with a rubber chicken? That's absurd.
    "It's called *alchemy*, thank you very much! Clearly, it's too advanced for you."
    -> quest_failed


=== task_9 ===
"Right!" says {get_name(pink_npc_id)}, rubbing their hands together. "Task 9: The Beast!
Deep within the larder... lurks the Guardian of the Pantry! A creature of myth... or possibly just a large rat.
You must vanquish it! You'll need your best weapon. Do you still have that Steel Sword?"
You currently have {count_item(player_id, "Sword (Steel)")} Steel Sword(s).
*  {count_item(player_id, "Sword (Steel)") > 0} [Check Sword] I have the Steel Sword! Point me to this 'Guardian'!
    Excellent! Go forth and secure the pantry! For... uh... snacks!
    You hear some scuffling, a squeak, then silence.
    Victory! The pantry is safe! You find this pouch of coins, clearly dropped by the 'Guardian'.
    ~ add_currency(player_id, 150) // Generous reward
    You now have {get_currency(player_id)} dollars. Well done!
    ++ Off to the final quest!
       >>> close
       -> task_10
* {count_item(player_id, "Sword (Steel)") == 0} [Check Sword] I seem to have misplaced my Steel Sword.
    "What?! Facing the Guardian without proper armament? Foolish! Find your sword!"
    ++ Now where did I put it?
       >>> close
       -> task_9
* Fighting pantry pests isn't my idea of adventure.
    "But the *snacks*, adventurer! Think of the snacks! Fine, suit yourself."
    -> quest_failed


=== task_10 ===
"You've done it!" {get_name(pink_npc_id)} exclaims. "Task 1 through 9, completed! Amazing!"
"Polishing rocks, escorting snails, finding pebbles,
delivering dubious boxes, deciphering Pig Latin, acquiring poultry,
brewing conceptual potions, defeating pantry vermin..."

"You must be wondering what grand destiny all this was for, what vital purpose it served?"
+   Yes! Tell me! What was it all for?
    -> task_10_reveal
+   Honestly, I stopped caring around Task 3.
    -> task_10_reveal


=== task_10_reveal ===
"Truth is," {get_name(pink_npc_id)} says with a wide grin,
"I was incredibly bored. And you looked like you needed something to do."

"There's no grand destiny, no world-saving purpose.
It was just... fun! For me, anyway."

"But you were a good sport! So, for completing all ten... tasks...
here is your final, magnificent reward!"

You have {get_currency(player_id)} dollars.
~ add_currency(player_id, 1) // The ultimate reward

"One dollar! Spend it wisely!"
You now have {get_currency(player_id)} dollars.

* ...One dollar? After all that?
    "Yep! But think of the *experience*! Priceless, right?"
    -> quest_failed
* [Sigh] Thanks, I guess. It certainly was... something.
    "That's the spirit! Now, if you'll excuse me, I need to think up some tasks for the *next* adventurer..."
    -> quest_failed

*/
