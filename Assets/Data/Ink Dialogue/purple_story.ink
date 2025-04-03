INCLUDE functions.ink

// We're using variable to track the quest state for demonstration purposes.
// We could just as well use the quest states of "purple_quest".

VAR quest_started = false
VAR talked_to_purple_npc = false

=== start ===
~ set_quest_status("purple_quest", "Active")
Hello, this is {get_npc_name()}, with ID {npc_id}.
My registered name is {get_name(npc_id)}.

You start with {get_currency(player_id)} dollars
<>and {count_item(player_id, "Pickaxe")} pickaxe(s).

Let's change this:

~ add_currency(player_id, 20)
~ add_item(player_id, "Pickaxe", 1)
~ temp number_of_pickaxes = count_item(player_id, "Pickaxe")

You now have {get_currency(player_id)} dollars and
<>{number_of_pickaxes} pickaxe(s).

+   p: OK. See you later.
    ~ set_task_status("purple_quest", "talk_to_pink_npc_1", "Active")
    >>> close

    Welcome back!
    {
        - number_of_pickaxes == count_item(player_id, "Pickaxe"):
            Your number of pickaxes hasn't changed.
        - else:
            You used to have {number_of_pickaxes} pickaxe(s),
            now you have {count_item(player_id, "Pickaxe")}.
    }
    -> quest_start

=== quest_start ===
~ quest_started = true
~ set_task_status("purple_quest", "talk_to_pink_npc_1", "Completed")
~ set_task_status("purple_quest", "talk_to_purple_npc", "Active")
Talk to {get_name(purple_npc_id)}.
They'll be somewhere around here.
+   p: OK.
    >>> close
    -> quest_check_talked_to_purple_npc

=== quest_check_talked_to_purple_npc ===
Did you talk to {get_name(purple_npc_id)}?
+   { talked_to_purple_npc } p: Sure did.
    Cool. Here is your reward.
    ~ add_currency(player_id, 100)
    ~ add_item(player_id, "Medium Health Potion", 2)
    ~ set_task_status("purple_quest", "talk_to_pink_npc_2", "Completed")
    ~ set_quest_status("purple_quest", "Completed")
    You now have {get_currency(player_id)} gold and {count_item(player_id, "Medium Health Potion")}
    <> Medium Health Potions.
    ++  p: Cool.
        >>> close
        -> quest_over
+   { not talked_to_purple_npc } p: Nope.
    Pity.
    ++  See ya!
        >>> close
        -> quest_check_talked_to_purple_npc

=== quest_over ===
I think we have nothing to discuss right now.
+   p: If you think so...
    >>> close
    -> quest_over


=== purple_npc_start ===
{ quest_started:
    -> talk_to_player
  - else:
    -> dont_talk_to_player
}

= talk_to_player
    Here is the secret phrase: "The crows feast rich tonight."
    + [Got it.] p: "The crows feast rich tonight." Got it.
      ~ talked_to_purple_npc = true
      ~ set_task_status("purple_quest", "talk_to_purple_npc", "Completed")
      ~ set_task_status("purple_quest", "talk_to_pink_npc_2", "Active")
      >>> close
      -> dont_talk_to_player

= dont_talk_to_player
    Go away!
    + p: How rude!
    >>> close
    -> purple_npc_start
