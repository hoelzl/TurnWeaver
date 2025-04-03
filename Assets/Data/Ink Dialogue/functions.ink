INCLUDE unique_ids.ink

EXTERNAL add_currency(unique_id, amount)
EXTERNAL add_item(unique_id, unique_item_name, quantity)
EXTERNAL can_add_item(unique_id, unique_item_name, quantity)
EXTERNAL count_item(unique_id, unique_item_name)
EXTERNAL get_currency(unique_id)
EXTERNAL get_name(unique_id)
EXTERNAL get_npc_name()
EXTERNAL get_quest_status(quest_short_name)
EXTERNAL get_task_status(quest_short_name, task_short_name)
EXTERNAL remove_currency(unique_id, amount)
EXTERNAL remove_item(unique_id, unique_item_name, quantity)
EXTERNAL set_quest_status(quest_short_name, status_string)
EXTERNAL set_task_status(quest_short_name, task_short_name, status_string)

=== function add_currency(unique_id, amount) ===
    <added currency>

=== function add_item(unique_id, unique_item_name, quantity) ===
    <added item>

=== function can_add_item(unique_id, unique_item_name, quantity) ===
    ~ return true

=== function count_item(unique_id, unique_item_name) ===
    ~ return -1

=== function get_currency(unique_id) ===
    ~ return -1

=== function get_name(unique_id) ===
    <the name of {unique_id}>

=== function get_npc_name() ===
    <the current NPC>

=== function get_quest_status(quest_short_name) ===
    <quest status of {quest_short_name}>

=== function get_task_status(quest_short_name, task_short_name) ===
    <task status of {quest_short_name}.{task_short_name}>

=== function remove_currency(unique_id, amount) ===
    <removed currency>

=== function remove_item(unique_id, unique_item_name, quantity) ===
    <removed item>

=== function set_quest_status(quest_short_name, status_string) ===
    <setting quest status>

=== function set_task_status(quest_short_name, task_short_name, status_string) ===
    <setting task status>
