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

-- Insert sample data into 'components'
INSERT INTO components (page_id, name, settings, ordinal)
VALUES
-- Components for Home Page
(1, 'Header', '{"background_color": "blue", "text": "Welcome"}', 1),
(1, 'Hero Banner', '{"image": "hero.jpg", "heading": "Welcome to our site"}', 2),
(1, 'Footer', '{"background_color": "gray", "text": "Contact Us"}', 3),

-- Components for About Us
(2, 'Header', '{"background_color": "green", "text": "About Us"}', 1),
(2, 'Mission Statement', '{"text": "Our mission is to innovate."}', 2),
(2, 'Footer', '{"background_color": "gray", "text": "Contact Us"}', 3),

-- Components for Contact Us
(3, 'Header', '{"background_color": "purple", "text": "Contact Us"}', 1),
(3, 'Contact Form', '{"fields": ["Name", "Email", "Message"]}', 2),
(3, 'Map', '{"coordinates": {"lat": 40.7128, "lon": -74.0060}}', 3),
(3, 'Footer', '{"background_color": "gray", "text": "Follow Us"}', 4),

-- Components for Services
(4, 'Header', '{"background_color": "orange", "text": "Services"}', 1),
(4, 'Service List', '{"services": ["Consulting", "Development", "Design"]}', 2),
(4, 'Testimonial', '{"text": "Amazing service!"}', 3),
(4, 'Footer', '{"background_color": "gray", "text": "Contact Us"}', 4),

-- Components for Blog
(5, 'Header', '{"background_color": "red", "text": "Blog"}', 1),
(5, 'Latest Posts', '{"posts": ["Post 1", "Post 2", "Post 3"]}', 2),
(5, 'Subscribe', '{"cta": "Sign up for updates"}', 3),
(5, 'Footer', '{"background_color": "gray", "text": "Follow Us"}', 4),

-- Components for Careers
(6, 'Header', '{"background_color": "teal", "text": "Careers"}', 1),
(6, 'Job Openings', '{"jobs": ["Software Engineer", "Product Manager"]}', 2),
(6, 'Apply Form', '{"fields": ["Name", "Resume"]}', 3),
(6, 'Footer', '{"background_color": "gray", "text": "Join Us"}', 4),

-- Components for FAQ
(7, 'Header', '{"background_color": "blue", "text": "FAQ"}', 1),
(7, 'FAQ List', '{"faqs": ["What is your refund policy?", "How can I contact support?"]}', 2),
(7, 'Footer', '{"background_color": "gray", "text": "Contact Us"}', 3),

-- Components for Portfolio
(8, 'Header', '{"background_color": "black", "text": "Portfolio"}', 1),
(8, 'Project Gallery', '{"projects": ["Project 1", "Project 2"]}', 2),
(8, 'Testimonial', '{"text": "Their work is fantastic!"}', 3),
(8, 'Footer', '{"background_color": "gray", "text": "Contact Us"}', 4),

-- Components for Terms of Service
(9, 'Header', '{"background_color": "darkgray", "text": "Terms of Service"}', 1),
(9, 'Terms Text', '{"sections": ["Section 1", "Section 2"]}', 2),
(9, 'Footer', '{"background_color": "gray", "text": "Contact Us"}', 3),

-- Components for Privacy Policy
(10, 'Header', '{"background_color": "darkgreen", "text": "Privacy Policy"}', 1),
(10, 'Privacy Policy Text', '{"sections": ["Section A", "Section B"]}', 2),
(10, 'Footer', '{"background_color": "gray", "text": "Contact Us"}', 3);
