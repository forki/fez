#!/usr/bin/env escript
% vi: ft=erlang


main([]) ->
    true = code:add_pathz(filename:dirname(escript:script_name())),
    io:format("-testing basics:flip~n"),
    "therehi" = basics:flip(fun erlang:'++'/2, "hi", "there"),

    io:format("-testing basics:try_head~n"),
    {'Some', 1} = basics:try_head([1]),
    {'None'} = basics:try_head([]),

    io:format("-testing basics:try_match_a_list~n"),
    {'Some', 3} = basics:try_match_a_list([1, 2, 3, 4, 5]),
    {'None'} = basics:try_match_a_list([1]),


    io:format("-testing basics records~n"),
    {person, "karl", 39} = P = basics:make_person("karl", 39),
    39 = basics:age(P),
    {person, "karl", 40} = basics:have_birthday(P),


    io:format("-testing basics:sum~n"),
    6 = basics:sum(3),
    0 = basics:sum(0),

    io:format("-testing basics:isOneToFour ~n"),
    "no" = basics:isOneToFour(0),
    "yes" = basics:isOneToFour(1),
    "yes" = basics:isOneToFour(2),
    "yes" = basics:isOneToFour(3),
    "yes" = basics:isOneToFour(4),
    "no" = basics:isOneToFour(5),

    io:format("-testing basics:tuple_matching~n"),
    "hi" = basics:tuple_matching(1, "hi"),
    "hi2hi2" = basics:tuple_matching(2, "hi2"),
    "threethree" = basics:tuple_matching(3, "three"),
    "many" = basics:tuple_matching(4, "four"),

    io:format("-testing basics:add ~n"),
    3 = basics:add(1, 2),
    % addFive returns a fun
    6 = (basics:addFive())(1),
    7 = basics:addSix(1),
    9 = (basics:addSeven())(2),
    10 = (basics:addEight())(2),

    io:format("-testing basics:(str|list)Len ~n"),
    0 = basics:strLen(""),
    4 = basics:strLen("asdf"),
    0 = basics:strLen2(""),
    4 = basics:strLen2("asdf"),
    0 = basics:listLen([]),
    4 = basics:strLen([1,2,3,4]),

    io:format("-testing basics:*ListModule ~n"),
    [2,3,4] = wip:addOneToAll([1,2,3]),
    6 = wip:foldTest([1,2,3]),
    6 = wip:foldTest2([1,2,3]),
    [1,2,3] = wip:sort([3,1,2]),

    io:format("OK: basics~n"),
    ok.
