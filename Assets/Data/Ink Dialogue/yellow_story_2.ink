INCLUDE functions.ink

VAR gave_wolf_pelts = false

=== start ===

{
    - get_task_status("yellow_quest", "wolf_pelts") == "Active" and not gave_wolf_pelts:
        -> start.offer_wolf_pelts
    - get_task_status("yellow_quest", "talk_to_other_npc") == "Active":
        -> lettuce_tasting
    - get_task_status("yellow_quest", "deliver_vibrating_package") == "Active":
        -> receive_vibrating_package
    - else:
        -> nothing_to_see
}

= nothing_to_see
Nothing to see here, I'm afraid
+   p: OK, then...
    >>> close
    -> start

= offer_wolf_pelts
You don't happen to know somebody who needs a few wolf pelts?
Like, 100, or so?
+   p: As it happens I do...
    Great! My whole storage room is full with these damned pelts.
    Here take them and be on your way. And don't expect me to pay you!
    ~ add_item(player_id, "Wolf Pelt", 100)
    ~ gave_wolf_pelts = true
    ++  p: Uh... thanks, I guess?
        >>> close
        -> start
+   p: I'm not sure...
    Damn. I'll never get rid of these damned pelts!
    ++  p: Sorry about that...
        >>> close
        -> start

= lettuce_tasting
We have so much delicious lettuce. Gary is going to be ecstatic!
+   p: That's good I guess...
    ~ set_task_status("yellow_quest", "talk_to_other_npc", "Completed")
    >>> close
    -> start

= receive_vibrating_package
Oohhh, I've been waiting for this for such a long time.
You did not peek inside, did you? {get_name(second_yellow_npc_id)}
<> admonishes you.
+   p: Of course I didn't!
    ~ set_task_status("yellow_quest", "deliver_vibrating_package", "Completed")
    I knew I could count on you!
    ++  p: Still, the contents was quite embarrassing...
        >>> close
        -> start
+   p: Well...
    ~ set_task_status("yellow_quest", "deliver_vibrating_package", "Failed")
    How... unfortunate, very disappointing!
    You will not be compensated!
    ++  p: Oh!
        >>> close
        -> start
