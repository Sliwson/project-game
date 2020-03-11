# README #
This is repository for IO2 MiNI course.

## Methodology
We are working in Scrum. But our Scrum is more agile then expected.
Product Owner is the same person as Project Manager, Team Leader, Architect and Lead Developer is one person - Mateusz is the one.
I decided to be a Scrum Master - Szymon.
Apart from that, we are all developers responsible mostly for specific modules:

* Game Master - Mateusz, Michał
* Agent - Marta, Weronika
* Communication server - Szymon

## Schedule
As we are working in Scrum, we have sprints. They are week or two long:
#### Sprint 1 (11.03 - 24.03) - Game (development)
Working in teams as mentioned above, in order to create the best experience for future users. That means spending two free weeks on coding.

#### Sprint 2 (25.03 - 31.03) - Game (testing)
Apart from unit and integration tests, we need more tests. So whole week for testing and fixing bugs. And adding more tests.

#### Sprint 3 (01.04 - 21.04) - Communication (development)
Communication is not always as simple as expected. So we have to handle lots of exceptions, serialization, deserialization, connection problems and so and so forth.

#### Sprint 4 (22.04 - 5.05) - Communication (tests)
See Sprint 2 - more testing, this time for functionalities implemented in Sprint 3.

#### Sprint 5 (06.05 - 12.05) - Cooperation (development)
It is always nice to know what to do. We do, but our agents don't. So we will work on teaching them about some fancy, extremely complicated tactics, communication strategy, reacting to expected problems and tricking opponents.

#### Sprint 6 (13.05 - 19.05) - Cooperation (testing)
Time for our agents to prove that they now know what to do. Fixing, learning more tactics, fighting with themselves (and each other). If one agent loses his mind, rest of the team should keep working.

#### Sprint 7 (20.05 - 27.05) - Preparations for Championship
The time of the trial is coming. We have to make sure our agents are ready to face other teams (or at least move). Final bootcamp, more practice, probably friendly matches with other teams - everything to cover our team with glory on the finest Championship these year is about to give us.

## Technology
* Agent, Communication server - .NET Core 2.1 console applications (pipelines support and portability)
* Game Master - .NET Framework 4.7.2 (for easier presentation layer integration) if we manage to deploy it in docker container. Otherwise, .NET Core 2.1 with simple presentation layer.
