INCLUDE functions.ink

=== start ===
A simple story told by {get_npc_name()}:

- I looked at Monsieur Fogg
+   p: ... and I could contain myself no longer.
    'What is the purpose of our journey, Monsieur?'
    'A wager,' he replied.
    + +     p: 'A wager!'[] I returned.
            He nodded.
            + + +  p: 'But surely that is foolishness!'
            + + +  p: 'A most serious matter then!'
            - - -   He nodded again.
            + + +  p: 'But can we win?'
                    >>> some-action:
                    'That is what we will endeavour to find out,' he answered.
            + + +   p: 'A modest wager, I trust?'
                    'Twenty thousand pounds,' he replied, quite flatly.
                    >>> some-other-action:
            + + +   p: I asked nothing further of him then[.], and after a final, polite cough, he offered nothing more to me. <>
    + +     p: 'Ah[.'],' I replied, uncertain what I thought.
    - -     After that, <>
+   p: ... but I said nothing[] and <>
- p: we passed the day in silence.
-> END
