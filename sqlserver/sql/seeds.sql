-- Insert sample data into 'pages'
INSERT INTO pages (title, slug, content)
VALUES
('Home Page', 'home', 'Welcome to our homepage!'),
('About Us', 'about-us', 'Learn more about our company.'),
('Contact Us', 'contact-us', 'Reach out to us here.'),
('Services', 'services', 'Our list of services.'),
('Blog', 'blog', 'Latest articles and updates.'),
('Careers', 'careers', 'Join our team.'),
('FAQ', 'faq', 'Frequently asked questions.'),
('Portfolio', 'portfolio', 'Our previous works and case studies.'),
('Terms of Service', 'terms-of-service', 'Our legal terms and conditions.'),
('Privacy Policy', 'privacy-policy', 'How we handle your data.');

-- Insert unique components into 'components'
INSERT INTO components (name, settings)
VALUES
('Header', '{"background_color": "blue", "text": "Welcome"}'),
('Hero Banner', '{"image": "hero.jpg", "heading": "Welcome to our site"}'),
('Footer', '{"background_color": "gray", "text": "Contact Us"}'),
('Mission Statement', '{"text": "Our mission is to innovate."}'),
('Contact Form', '{"fields": ["Name", "Email", "Message"]}'),
('Map', '{"coordinates": {"lat": 40.7128, "lon": -74.0060}}'),
('Service List', '{"services": ["Consulting", "Development", "Design"]}'),
('Testimonial', '{"text": "Amazing service!"}'),
('Latest Posts', '{"posts": ["Post 1", "Post 2", "Post 3"]}'),
('Subscribe', '{"cta": "Sign up for updates"}'),
('Job Openings', '{"jobs": ["Software Engineer", "Product Manager"]}'),
('Apply Form', '{"fields": ["Name", "Resume"]}'),
('FAQ List', '{"faqs": ["What is your refund policy?", "How can I contact support?"]}'),
('Project Gallery', '{"projects": ["Project 1", "Project 2"]}'),
('Terms Text', '{"sections": ["Section 1", "Section 2"]}'),
('Privacy Policy Text', '{"sections": ["Section A", "Section B"]}');

-- Insert relationships into 'page_components'
INSERT INTO page_components (page_id, component_id, ordinal)
VALUES
-- Components for Home Page
(1, (SELECT id FROM components WHERE name = 'Header'), 1),
(1, (SELECT id FROM components WHERE name = 'Hero Banner'), 2),
(1, (SELECT id FROM components WHERE name = 'Footer'), 3),

-- Components for About Us
(2, (SELECT id FROM components WHERE name = 'Header'), 1),
(2, (SELECT id FROM components WHERE name = 'Mission Statement'), 2),
(2, (SELECT id FROM components WHERE name = 'Footer'), 3),

-- Components for Contact Us
(3, (SELECT id FROM components WHERE name = 'Header'), 1),
(3, (SELECT id FROM components WHERE name = 'Contact Form'), 2),
(3, (SELECT id FROM components WHERE name = 'Map'), 3),
(3, (SELECT id FROM components WHERE name = 'Footer'), 4),

-- Components for Services
(4, (SELECT id FROM components WHERE name = 'Header'), 1),
(4, (SELECT id FROM components WHERE name = 'Service List'), 2),
(4, (SELECT id FROM components WHERE name = 'Testimonial'), 3),
(4, (SELECT id FROM components WHERE name = 'Footer'), 4),

-- Components for Blog
(5, (SELECT id FROM components WHERE name = 'Header'), 1),
(5, (SELECT id FROM components WHERE name = 'Latest Posts'), 2),
(5, (SELECT id FROM components WHERE name = 'Subscribe'), 3),
(5, (SELECT id FROM components WHERE name = 'Footer'), 4),

-- Components for Careers
(6, (SELECT id FROM components WHERE name = 'Header'), 1),
(6, (SELECT id FROM components WHERE name = 'Job Openings'), 2),
(6, (SELECT id FROM components WHERE name = 'Apply Form'), 3),
(6, (SELECT id FROM components WHERE name = 'Footer'), 4),

-- Components for FAQ
(7, (SELECT id FROM components WHERE name = 'Header'), 1),
(7, (SELECT id FROM components WHERE name = 'FAQ List'), 2),
(7, (SELECT id FROM components WHERE name = 'Footer'), 3),

-- Components for Portfolio
(8, (SELECT id FROM components WHERE name = 'Header'), 1),
(8, (SELECT id FROM components WHERE name = 'Project Gallery'), 2),
(8, (SELECT id FROM components WHERE name = 'Testimonial'), 3),
(8, (SELECT id FROM components WHERE name = 'Footer'), 4),

-- Components for Terms of Service
(9, (SELECT id FROM components WHERE name = 'Header'), 1),
(9, (SELECT id FROM components WHERE name = 'Terms Text'), 2),
(9, (SELECT id FROM components WHERE name = 'Footer'), 3),

-- Components for Privacy Policy
(10, (SELECT id FROM components WHERE name = 'Header'), 1),
(10, (SELECT id FROM components WHERE name = 'Privacy Policy Text'), 2),
(10, (SELECT id FROM components WHERE name = 'Footer'), 3);