# DND-semester-Project
Shared repo for .net development course project - Group work.
Usernames:
- Ceciliejs    (316126)
- Sigurdhorsholt    (303530)
- Jonasbj8    (316124)
- Rasm105j    (316137)
- MarkusBM   (316148)


# 1. Blogpost - Project Formulation & Requirements



## Shared Laundrymat Online booking system

####  Description

The general idea is a system that allows users in an apartement complex with shared laundry system in the complex to book slots from their couch instead of going to the basement. 

The value proposition is that it sucks to go to the basement twice. First to book the slot and then later to do the laundry. You might even get 3 trips if no times are available.

**The system overview**
**Backend:**
- Multible users
-- SysAdmin for adding new apartement complexes and adding complex admins
-- Complex Admin for adding time slots, machines and users for his/hers complex
-- Daily users - can only manage their own password and book/delete time slots.

- Laundry room
-- laundry room entity
-- Laundry machine entity
-- Drying machine entity

- Time entities
-- Booking - Complex admin can vary length of booking slots for indiviudal complex
-- Bookings will be available to users as time slots and can be available/unavailable
-- Bookings cannot overlap an should be locked
-- When a user is booking the time slot should be locked for other users UNTIL the user confirms his/hers booking. This needs a locked functionlaity in DB that will be dependent on the users confirmation
-- A systemAdmin can configure how many bookings each user is allowed to have 


- Config site backend
-- Admin config site to setup: bookings, timeslots, number of allowed bookings, Number of available machines etc.

- Auth system
-- Secure Login feature
-- Password reset system (maybe through email)
-- Users should be registered through accept of the complex admin
-- Users should be allowed to change email and username but should be locked to an appartment number that users themselves cannot change. This way the admin doesnt need to help when new people move into appartment and the account needs to change hands...

**FrontEnd:**

- Log in page
-- Register / forgot password / login
- Homepage
-- This is where users current bookings are visible
-- Make new booking
-- Should generate based on how complex admin have configured complex
-- Available and booked time slots. 
-- Wether users can see Which other apartment has booked a time slots should be set in config for privacy reasons. 
- add / delete booking 
- Manage user page
--  Change: username, password, email.

**User Stories:**
The user stories will cover the range of requirements for different user roles in the laundry booking system.

Daily Users:
- *"As a user, I want to log into the system so that I can access the laundy booking system"*
- *"As a User, I want to view my current and upcoming laundry bookings on the homepage so that I can manage my laundry schedule effectively"*
- *"As a User, I want to scroll through available laundry time slots so that I can choose a time that fits me to do my laundry."*
- *"As a User, I want the system to temporarily lock the time slot I am selecting during the booking process so that no one else can book it until I confirm or cancel."*
- *"As a User, I want to receive a confirmation of my booking once I finalize it so that I know my slot is successfully reserved."*
- *"As a User, I want to cancel or modify my laundry booking so that I can free up the time slot or reschedule my laundry if needed."*
- *"As a User, I want to see a visual indicator of which slots are available and which are booked so that I can plan my laundry based on the available time."*
- *"As a User, I want to manage my account details, such as changing my username, email, or password"*



Admin Users:
- *"As an admin, I want to be able to see a dashboard with settings that normal users can't see when they log in"*
-	*"As an admin, I want to create, update, or delete user accounts so that I can manage user access to the system effectively."*
- *"As an admin, I want to add, modify, or remove laundry time slots so that I can ensure the booking system reflects the correct availability."*
-	*"As an admin, I want to override and cancel a user's booking in special circumstances so that I can manage conflicts or errors in the schedule."*

- *"As an admin, I want to configure and manage settings for each laundry room individually in the case i have to be an Admin for multible rooms.."*
- *"As an admin, I want to manage Users for each Laundry room. I want to be able to register new users to a laundry room and i want to be able to delete users that should have their access removed.."*

