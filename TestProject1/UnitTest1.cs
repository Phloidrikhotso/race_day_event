using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using race_day_event.Controllers;
using System.Diagnostics.CodeAnalysis;

namespace TestProject1
{
    public class UnitTest1
    {
        // Helper: gives a controller a fake session with an optional user id and role
        private static DefaultHttpContext set_session(ControllerBase controller, int? user_id, string? role)
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Session = new fake_session();

            if (user_id != null)
            {
                context.Session.SetInt32("userID", user_id.Value);
            }

            if (role != null)
            {
                context.Session.SetString("role", role);
            }

            controller.ControllerContext = new ControllerContext { HttpContext = context };

            return context;
        }

        // ---------------- Events controller ----------------

        [Fact]
        public void create_event_without_session_returns_401()
        {
            events controller = new events();
            set_session(controller, null, null);

            string result = controller.create_event("Test Run", "", DateTime.Now.AddDays(10),
                "Pretoria", 10, events.event_type_choice.Run);

            Assert.Equal(401, controller.Response.StatusCode);
            Assert.Equal("No active session...", result);
        }

        [Fact]
        public void create_event_as_participant_returns_403()
        {
            events controller = new events();
            set_session(controller, 200001, "Participant");

            controller.create_event("Test Run", "", DateTime.Now.AddDays(10),
                "Pretoria", 10, events.event_type_choice.Run);

            Assert.Equal(403, controller.Response.StatusCode);
        }

        [Fact]
        public void create_event_with_zero_distance_returns_400()
        {
            events controller = new events();
            set_session(controller, 200001, "Organizer");

            controller.create_event("Test Run", "", DateTime.Now.AddDays(10),
                "Pretoria", 0, events.event_type_choice.Run);

            Assert.Equal(400, controller.Response.StatusCode);
        }

        [Fact]
        public void create_event_with_blank_name_returns_400()
        {
            events controller = new events();
            set_session(controller, 200001, "Organizer");

            controller.create_event("", "", DateTime.Now.AddDays(10),
                "Pretoria", 10, events.event_type_choice.Run);

            Assert.Equal(400, controller.Response.StatusCode);
        }

        [Fact]
        public void update_event_without_session_returns_401()
        {
            events controller = new events();
            set_session(controller, null, null);

            controller.update_event(300000, "Test Run", "", DateTime.Now.AddDays(10),
                "Pretoria", 10, events.event_type_choice.Run);

            Assert.Equal(401, controller.Response.StatusCode);
        }

        [Fact]
        public void update_event_as_participant_returns_403()
        {
            events controller = new events();
            set_session(controller, 200001, "Participant");

            controller.update_event(300000, "Test Run", "", DateTime.Now.AddDays(10),
                "Pretoria", 10, events.event_type_choice.Run);

            Assert.Equal(403, controller.Response.StatusCode);
        }

        [Fact]
        public void delete_event_without_session_returns_401()
        {
            events controller = new events();
            set_session(controller, null, null);

            string result = controller.delete_event(300000);

            Assert.Equal(401, controller.Response.StatusCode);
            Assert.Equal("No active session...", result);
        }

        [Fact]
        public void delete_event_as_participant_returns_403()
        {
            events controller = new events();
            set_session(controller, 200001, "Participant");

            controller.delete_event(300000);

            Assert.Equal(403, controller.Response.StatusCode);
        }

        [Fact]
        public void get_my_events_without_session_returns_401()
        {
            events controller = new events();
            set_session(controller, null, null);

            controller.get_my_events();

            Assert.Equal(401, controller.Response.StatusCode);
        }

        [Fact]
        public void get_my_events_as_participant_returns_403()
        {
            events controller = new events();
            set_session(controller, 200001, "Participant");

            controller.get_my_events();

            Assert.Equal(403, controller.Response.StatusCode);
        }

        // ---------------- Categories controller ----------------

        [Fact]
        public void create_category_without_session_returns_401()
        {
            categories controller = new categories();
            set_session(controller, null, null);

            controller.create_category(300000, categories.category_name_choice.Run, 100, 10, 200);

            Assert.Equal(401, controller.Response.StatusCode);
        }

        [Fact]
        public void create_category_as_participant_returns_403()
        {
            categories controller = new categories();
            set_session(controller, 200001, "Participant");

            controller.create_category(300000, categories.category_name_choice.Run, 100, 10, 200);

            Assert.Equal(403, controller.Response.StatusCode);
        }

        [Fact]
        public void create_category_with_negative_fee_returns_400()
        {
            categories controller = new categories();
            set_session(controller, 200001, "Organizer");

            controller.create_category(300000, categories.category_name_choice.Run, -50, 10, 200);

            Assert.Equal(400, controller.Response.StatusCode);
        }

        [Fact]
        public void create_category_with_zero_distance_returns_400()
        {
            categories controller = new categories();
            set_session(controller, 200001, "Organizer");

            controller.create_category(300000, categories.category_name_choice.Run, 100, 0, 200);

            Assert.Equal(400, controller.Response.StatusCode);
        }

        [Fact]
        public void create_category_with_zero_max_participants_returns_400()
        {
            categories controller = new categories();
            set_session(controller, 200001, "Organizer");

            controller.create_category(300000, categories.category_name_choice.Run, 100, 10, 0);

            Assert.Equal(400, controller.Response.StatusCode);
        }

        [Fact]
        public void update_category_as_participant_returns_403()
        {
            categories controller = new categories();
            set_session(controller, 200001, "Participant");

            controller.update_category(400000, categories.category_name_choice.Walk, 100, 5, 200);

            Assert.Equal(403, controller.Response.StatusCode);
        }

        [Fact]
        public void delete_category_without_session_returns_401()
        {
            categories controller = new categories();
            set_session(controller, null, null);

            string result = controller.delete_category(400000);

            Assert.Equal(401, controller.Response.StatusCode);
            Assert.Equal("No active session...", result);
        }

        // ---------------- Enrolments controller ----------------

        [Fact]
        public void enrol_without_session_returns_401()
        {
            enrolments controller = new enrolments();
            set_session(controller, null, null);

            controller.enrol_in_event(300000, 400000);

            Assert.Equal(401, controller.Response.StatusCode);
        }

        [Fact]
        public void enrol_as_organizer_returns_403()
        {
            enrolments controller = new enrolments();
            set_session(controller, 200001, "Organizer");

            controller.enrol_in_event(300000, 400000);

            Assert.Equal(403, controller.Response.StatusCode);
        }

        [Fact]
        public void get_my_enrolments_as_organizer_returns_403()
        {
            enrolments controller = new enrolments();
            set_session(controller, 200001, "Organizer");

            controller.get_my_enrolments();

            Assert.Equal(403, controller.Response.StatusCode);
        }

        [Fact]
        public void get_event_enrolments_as_participant_returns_403()
        {
            enrolments controller = new enrolments();
            set_session(controller, 200002, "Participant");

            controller.get_event_enrolments(300000);

            Assert.Equal(403, controller.Response.StatusCode);
        }

        [Fact]
        public void cancel_enrolment_without_session_returns_401()
        {
            enrolments controller = new enrolments();
            set_session(controller, null, null);

            string result = controller.cancel_enrolment(500000);

            Assert.Equal(401, controller.Response.StatusCode);
            Assert.Equal("No active session...", result);
        }

        // ---------------- Results controller ----------------

        [Fact]
        public void capture_result_as_participant_returns_403()
        {
            results controller = new results();
            set_session(controller, 200002, "Participant");

            controller.capture_result(500000, TimeSpan.FromMinutes(45), 1, 1, "Finished");

            Assert.Equal(403, controller.Response.StatusCode);
        }

        [Fact]
        public void capture_result_with_invalid_status_returns_400()
        {
            results controller = new results();
            set_session(controller, 200001, "Organizer");

            controller.capture_result(500000, TimeSpan.FromMinutes(45), 1, 1, "Winner");

            Assert.Equal(400, controller.Response.StatusCode);
        }

        [Fact]
        public void capture_result_with_zero_position_returns_400()
        {
            results controller = new results();
            set_session(controller, 200001, "Organizer");

            controller.capture_result(500000, TimeSpan.FromMinutes(45), 0, 1, "Finished");

            Assert.Equal(400, controller.Response.StatusCode);
        }

        [Fact]
        public void capture_result_with_time_over_24_hours_returns_400()
        {
            results controller = new results();
            set_session(controller, 200001, "Organizer");

            controller.capture_result(500000, TimeSpan.FromHours(25), 1, 1, "Finished");

            Assert.Equal(400, controller.Response.StatusCode);
        }

        [Fact]
        public void get_my_results_as_organizer_returns_403()
        {
            results controller = new results();
            set_session(controller, 200001, "Organizer");

            controller.get_my_results();

            Assert.Equal(403, controller.Response.StatusCode);
        }

        [Fact]
        public void get_event_results_without_session_returns_401()
        {
            results controller = new results();
            set_session(controller, null, null);

            controller.get_event_results(300000);

            Assert.Equal(401, controller.Response.StatusCode);
        }

        // ---------------- Users controller ----------------

        [Fact]
        public void get_my_profile_without_session_returns_401()
        {
            users controller = new users();
            set_session(controller, null, null);

            string result = controller.get_my_profile();

            Assert.Equal(401, controller.Response.StatusCode);
            Assert.Equal("No active session...", result);
        }

        [Fact]
        public void update_my_profile_with_blank_first_name_returns_400()
        {
            users controller = new users();
            set_session(controller, 200001, "Participant");

            controller.update_my_profile("", "Smith", "jsmith", "j@test.com", "0712345678");

            Assert.Equal(400, controller.Response.StatusCode);
        }

        // ---------------- Authenticator controller ----------------

        [Fact]
        public void logout_clears_the_session()
        {
            authenticator controller = new authenticator();
            DefaultHttpContext context = set_session(controller, 200001, "Participant");

            string result = controller.logout_user();

            Assert.Equal("Logout method reached successfully...", result);
            Assert.Null(context.Session.GetInt32("userID"));
            Assert.Null(context.Session.GetString("role"));
        }
    }

    // A simple in-memory session used only by the tests above
    public class fake_session : ISession
    {
        private Dictionary<string, byte[]> store = new Dictionary<string, byte[]>();

        public string Id => "test-session";

        public bool IsAvailable => true;

        public IEnumerable<string> Keys => store.Keys;

        public void Clear()
        {
            store.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            store.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            store[key] = value;
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
        {
            return store.TryGetValue(key, out value);
        }
    }
}