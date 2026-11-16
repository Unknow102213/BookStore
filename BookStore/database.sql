
USE master;
GO

-- =============================================
-- 0. CREATE DATABASE
-- =============================================
IF DB_ID(N'BookStore') IS NOT NULL
BEGIN
    ALTER DATABASE BookStore SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BookStore;
END
GO

CREATE DATABASE BookStore;
GO

USE BookStore;
GO

-- =============================================
-- 1. TABLE: Users
-- =============================================
CREATE TABLE dbo.Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role CHAR(1) NOT NULL
        CONSTRAINT DF_Users_Role DEFAULT ('C'),
    IsActive BIT NOT NULL
        CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSDATETIME()),
    FullName NVARCHAR(100) NOT NULL,
    Phone VARCHAR(15) NULL,
    Address NVARCHAR(500) NULL,

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('C', 'A'))
);
GO

-- =============================================
-- 2. TABLE: Category
-- =============================================
CREATE TABLE dbo.Category
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE
);
GO

-- =============================================
-- 3. INSERT CATEGORIES
-- =============================================
INSERT INTO dbo.Category (CategoryName)
VALUES
(N'Sách Kinh Tế'),
(N'Sách Thiếu Nhi'),
(N'Sách Tâm Lý – Kỹ Năng Sống'),
(N'Sách Ngoại Ngữ'),
(N'Sách Giáo Khoa – Tham Khảo'),
(N'Sách Công Nghệ – Tin Học'),
(N'Sách Khoa Học – Đời Sống'),
(N'Sách Lịch Sử – Địa Lý'),
(N'Sách Văn Hóa – Nghệ Thuật'),
(N'Sách Chính Trị – Pháp Luật'),
(N'Sách Y Học – Sức Khỏe'),
(N'Truyện Tranh'),
(N'Từ Điển'),
(N'Sách Tôn Giáo – Tâm Linh'),
(N'Sách Văn Học Nước Ngoài'),
(N'Sách Văn Học Việt Nam'),
(N'Tiểu thuyết'),
(N'Truyện trinh thám'),
(N'Tiểu sử – Hồi ký');
GO

-- =============================================
-- 4. TABLE: Book
-- =============================================
CREATE TABLE dbo.Book
(
    BookID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    AuthorName NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL
        CONSTRAINT DF_Book_StockQuantity DEFAULT (0),
    ImageURL NVARCHAR(500) NULL,
    IsActive BIT NOT NULL
        CONSTRAINT DF_Book_IsActive DEFAULT (1),
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Book_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT FK_Book_Category
        FOREIGN KEY (CategoryID)
        REFERENCES dbo.Category(CategoryID),

    CONSTRAINT CK_Book_Price
        CHECK (Price >= 0),

    CONSTRAINT CK_Book_StockQuantity
        CHECK (StockQuantity >= 0)
);
GO

-- =============================================
-- 5. TABLE: Cart
-- =============================================
CREATE TABLE dbo.Cart
(
    CartID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Cart_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT UQ_Cart_UserID UNIQUE (UserID),

    CONSTRAINT FK_Cart_Users
        FOREIGN KEY (UserID)
        REFERENCES dbo.Users(UserID)
);
GO

-- =============================================
-- 6. TABLE: CartItem
-- =============================================
CREATE TABLE dbo.CartItem
(
    CartItemID INT IDENTITY(1,1) PRIMARY KEY,
    CartID INT NOT NULL,
    BookID INT NOT NULL,
    Quantity INT NOT NULL,

    CONSTRAINT UQ_CartItem_Cart_Book
        UNIQUE (CartID, BookID),

    CONSTRAINT FK_CartItem_Cart
        FOREIGN KEY (CartID)
        REFERENCES dbo.Cart(CartID),

    CONSTRAINT FK_CartItem_Book
        FOREIGN KEY (BookID)
        REFERENCES dbo.Book(BookID),

    CONSTRAINT CK_CartItem_Quantity
        CHECK (Quantity > 0)
);
GO

-- =============================================
-- 7. TABLE: Orders
-- =============================================
CREATE TABLE dbo.Orders
(
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    OrderDate DATETIME2 NOT NULL
        CONSTRAINT DF_Orders_OrderDate DEFAULT (SYSDATETIME()),
    TotalAmount DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Orders_TotalAmount DEFAULT (0),
    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Orders_Status DEFAULT (N'Chờ xác nhận'),
    PaymentMethod NVARCHAR(20) NOT NULL,
    PaymentStatus NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Orders_PaymentStatus DEFAULT (N'Chưa thanh toán'),
    ReceiverName NVARCHAR(100) NOT NULL,
    ReceiverPhone VARCHAR(15) NOT NULL,
    AddressDelivery NVARCHAR(255) NOT NULL,
    Notes NVARCHAR(500) NULL,
    CompletedAt DATETIME2 NULL,
    ReturnDeadline DATETIME2 NULL,

    CONSTRAINT FK_Orders_Users
        FOREIGN KEY (UserID)
        REFERENCES dbo.Users(UserID),

    CONSTRAINT CK_Orders_TotalAmount
        CHECK (TotalAmount >= 0)
);
GO

-- =============================================
-- 8. TABLE: OrderDetail
-- =============================================
CREATE TABLE dbo.OrderDetail
(
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    BookID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,

    CONSTRAINT UQ_OrderDetail_Order_Book
        UNIQUE (OrderID, BookID),

    CONSTRAINT FK_OrderDetail_Orders
        FOREIGN KEY (OrderID)
        REFERENCES dbo.Orders(OrderID),

    CONSTRAINT FK_OrderDetail_Book
        FOREIGN KEY (BookID)
        REFERENCES dbo.Book(BookID),

    CONSTRAINT CK_OrderDetail_Quantity
        CHECK (Quantity > 0),

    CONSTRAINT CK_OrderDetail_UnitPrice
        CHECK (UnitPrice >= 0)
);
GO

-- =============================================
-- 9. TABLE: Review
-- =============================================
CREATE TABLE dbo.Review
(
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    BookID INT NOT NULL,
    OrderID INT NOT NULL,
    Rating INT NOT NULL,
    Comment NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Review_CreatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT FK_Review_Users
        FOREIGN KEY (UserID)
        REFERENCES dbo.Users(UserID),

    CONSTRAINT FK_Review_Book
        FOREIGN KEY (BookID)
        REFERENCES dbo.Book(BookID),

    CONSTRAINT FK_Review_Orders
        FOREIGN KEY (OrderID)
        REFERENCES dbo.Orders(OrderID),

    CONSTRAINT UQ_Review_Order_Book
        UNIQUE (OrderID, BookID),

    CONSTRAINT CK_Review_Rating
        CHECK (Rating BETWEEN 1 AND 5)
);
GO

-- =============================================
-- 10. INSERT 20 BOOKS
-- =============================================
INSERT INTO dbo.Book
(
    CategoryID,
    Title,
    AuthorName,
    Description,
    Price,
    StockQuantity,
    ImageURL,
    IsActive
)
VALUES

(3, N'Giá Trị Của Sự Tử Tế', N'Piero Ferrucci',
N'Lòng tốt và sự đồng cảm là hai trong số nhiều điều thiết yếu khiến cuộc sống của chúng ta có ý nghĩa. Chúng là khởi nguồn cho hạnh phúc và niềm vui vĩnh cửu. Chúng là nền tảng của một trái tim nồng ấm, trái tim của người sẵn lòng dang tay giúp đỡ người khác một cách vô tư. Bằng lòng tốt, tình thương yêu, sự trung thực, chân thành và công bằng với tất cả mọi người, ta có thể bảo vệ sự tử tế. Đó là lẽ thường tình.<br><br>Quan tâm tới mọi người là một việc đáng làm, bởi vì hạnh phúc của chúng ta gắn liền với hạnh phúc của những người khác. Nếu xã hội chìm trong đau khổ, bản thân chúng ta cũng đau khổ. Khi trái tim và tâm trí ta càng bị cái ác lấn át, ta càng trở nên bất hạnh. Bởi vậy, lòng tốt và sự đồng cảm là những giá trị ta không thể thiếu được.<br><br>Việc cố gắng tạo ra điều gì đó tích cực trong cuộc sống là điều vô cùng quan trọng. Chúng ta không được sinh ra để gây rắc rối và làm hại người khác. Như Piero Ferrucci đã trình bày một cách đầy đủ và mạch lạc trong cuốn sách Giá trị của sự tử tế này, để cuộc sống thật sự có giá trị, ta cần củng cố và nuôi dưỡng những giá trị cơ bản của con người như sự nhiệt thành, lòng tốt và sự đồng cảm. Nếu làm được điều đó, cuộc sống của chúng ta sẽ trở nên có ý nghĩa, hạnh phúc và bình an hơn, đồng thời sẽ góp một phần tích cực vào thế giới quanh mình.',
120000, 100, N'~/Content/img/gia-tri-cua-su-tu-te.webp', 1),

(16, N'Than Ấm Bừng Lên', N'Lam',
N'Đôi khi, giữa những bộn bề hôm nay, ta chỉ muốn chạm vào chút ấm áp xưa cũ - như hơi than hồng trong một chiều gió lạnh, như tiếng bà gọi vọng từ căn bếp nhỏ. “Than Ấm Bừng Lên” của tác giả Lam đưa ta trở lại nơi ấy, nhẹ nhàng mà trọn vẹn.<br><br>Nếu như "Trốn lên mái nhà để khóc" đã từng chạm đến trái tim HÀNG VẠN ĐỘC GIẢ bằng cách tiếp thêm cho chúng ta can đảm để đối mặt với những tổn thương, tìm lại bình yên cho tâm hồn và “Thưa ngoại con mới về” là bức thư dài chan chứa nỗi nhớ thương dành cho ngoại và mái nhà quê yêu dấu, thì “Than ấm bừng lên” lại giống như một bếp lửa hồng êm dịu, sưởi ấm cả một miền ký ức tuổi thơ.<br><br>Tác phẩm tập hợp những bài thơ nhỏ, giản dị nhưng thấm đẫm tình cảm: buổi chiều mùa đông bà nhóm bếp than, mùi khói len qua giàn mướp chín gọi chân trẻ về; buổi tan học được ông đón trên con đường vàng nắng; nồi canh thơm nghi ngút mùi quê nhà; những mùa quả chín đợi người xa trở lại… Mỗi bài thơ là một khung ảnh chan chứa hơi ấm, khắc họa hình ảnh ông bà, cha mẹ và cả những góc vườn nhà, con vật… tất cả tạo thành bức tranh làng quê mộc mạc nhưng sống động. Những trang sách như một bếp than hồng, vừa ấm áp, vừa lung linh ánh sáng của yêu thương. Ta thấy ở đó tiếng lòng của những đứa cháu đã đi xa, mang trong mình nỗi nhớ khôn nguôi về hơi ấm và tình thương của bà:<br><br>“Bà ơi, con chẳng hứa suông<br><br>Chỉ là ngọn khói nó buồn, nó đau<br><br>Nó không nhóm bếp cho mau<br><br>Nó ngồi thút thít ở sau hiên nhà<br><br>Nó làm con cũng nhớ bà của con…”<br><br>Điều đặc biệt là Lam đã khéo léo đưa vào những bài học nhẹ nhàng về yêu thương, trân trọng, và gìn giữ những giá trị tốt đẹp. Lời thơ giản dị, giàu hình ảnh, đôi lúc hồn nhiên như tiếng trẻ con kể chuyện, đôi lúc lại trầm lắng như lời bà thủ thỉ bên bếp lửa.<br><br>Tập thơ sẽ dành cho bạn trong những chiều thư thả, để đọc chậm, để nhâm nhi từng câu chữ và để nhớ rằng hạnh phúc đôi khi nằm gọn trong những điều nhỏ bé nhất.<br><br>Khép lại “Than ấm bừng lên”, ta như vừa ngồi bên bếp lửa của tuổi thơ để hơi ấm len qua từng kẽ tay, để mùi khói vương trên vạt áo. Và khi bước ra đời, ta vẫn mang theo ngọn lửa ấy - một ngọn lửa của yêu thương, để tự sưởi ấm mình và để ủ ấm những người mà ta yêu thương.',
105000, 100, N'~/Content/img/than-am-bung-len.webp', 1),

(16, N'Dear, Người Yêu Cũ', N'Cammpodcast, Đỗ Quỳnh',
N'DEAR, NGƯỜI YÊU CŨ – Cuốn sách dành cho những trái tim đang học cách buông tay.<br><br>Nếu bạn từng có một người khiến mình mất rất lâu để quên, từng tự hỏi “Tại sao họ có thể bước tiếp còn mình thì vẫn mắc kẹt?”, Dear, người yêu cũ có lẽ sẽ chạm đến một góc rất quen thuộc trong bạn.',
86000, 100, N'~/Content/img/dear-nguoi-yeu-cu.webp', 1),

(16, N'Số Đỏ', N'Vũ Trọng Phụng',
N'“- Tôi không muốn lấy cái người ấy, vì nếu tôi lấy hắn, chắc hắn sẽ mọc sừng. Thí dụ một người như ông mà hỏi tôi thì còn nói gì! Đằng này hắn lại nhà quê, không biết yêu vợ như những người văn minh. Buồn lắm anh ạ.<br><br>- Tôi phải làm gì?<br><br>- Phải giả vờ chim tôi..., chúng ta giả vờ chim nhau, mê nhau... Cho hắn bỏ tôi, mình hiểu chưa? Ta giả vờ với nhau thôi mà! Tôi cần mang tiếng hư hỏng lắm mới được.<br><br>- Thế sao nữa ạ?<br><br>- Anh thì anh cũng phải mang tiếng là làm hại một đời tôi mới xong!”',
98000, 100, N'~/Content/img/so-do.webp', 1),

(1, N'Doanh Nghiệp Tự Hành', N'Mike Michalowicz',
N'Bạn lo lắng doanh nghiệp sẽ sụp đổ nếu thiếu vắng mình một giây? Bạn quyết định hy sinh gia đình, tình bạn và cả sự tự do để duy trì doanh nghiệp? Hãy thử đảo ngược một chút, nếu doanh nghiệp có thể tự động vận hành và phát triển, trong khi bạn có thể tự do làm những gì mình thích bất kỳ lúc nào, mọi chuyện sẽ ra sao?<br><br>Viễn cảnh này hoàn toàn có thể xảy ra. Thậm chí nó còn dễ dàng thực hiện hơn bạn tưởng tượng.',
139000, 100, N'~/Content/img/clock-work.webp', 1),

(1, N'Lãnh Đạo Bằng Sức Mạnh Trí Tuệ Cảm Xúc', N'Daniel Goleman, Richard Boyatzis, Annie Mckee',
N'Định kiến về sự phiền nhiễu của cảm xúc tại nơi làm việc đã cắm rễ quá sâu trong suy nghĩ của hầu hết chúng ta. Trên thực tế, mặc dù năng lực nhận thức thuần túy và năng lực chuyên môn là cần thiết cho công việc, nhưng chính các năng lực trí nhớ cảm xúc mới giúp phân định ra nhà lãnh đạo xuất chúng.<br><br>Những nghiên cứu mang tính đột phá về não bộ đã cho thấy sức mạnh của nhà lãnh đạo giàu trí tuệ cảm xúc trong việc truyền cảm hứng, khơi dậy cảm xúc tích cực và nhiệt tình cống hiến nơi nhân viên.<br><br>Tạp chí Time nhận xét về nội dung sách: “Cũng như cách mà cuốn sách đầu tiên của Goleman, Emotional Intelligence, mở rộng tầm nhìn của chúng ta về trí thông minh cảm xúc, Lãnh đạo bằng sức mạnh trí tuệ cảm xúc thay đổi gần như hoàn toàn các quan điểm trước nay về một nhà lãnh đạo tài ba.”',
148000, 100, N'~/Content/img/lanh-dao-bang-suc-manh-va-tri-tue-cam-xuc.webp', 1),

(3, N'Thiên Tài Bên Trái, Kẻ Điên Bên Phải', N'Cao Minh',
N'Thiên tài bên trái, kẻ điên bên phải là cuốn sách dành cho những người điên rồ, những kẻ gây rối, những người chống đối, những mảnh ghép hình tròn trong những ô vuông không vừa vặn… những người nhìn mọi thứ khác biệt, không quan tâm đến quy tắc. Bạn có thể đồng ý, có thể phản đối, có thể vinh danh hay lăng mạ họ, nhưng điều duy nhất bạn không thể làm là phủ nhận sự tồn tại của họ. Đó là những người luôn tạo ra sự thay đổi trong khi hầu hết con người chỉ sống rập khuôn như một cái máy. Đa số đều nghĩ họ thật điên rồ nhưng nếu nhìn ở góc khác, ta lại thấy họ thiên tài. Bởi chỉ những người đủ điên nghĩ rằng họ có thể thay đổi thế giới mới là những người làm được điều đó.',
179000, 100, N'~/Content/img/thien-tai-ben-trai-ke-dien-ben-phai.webp', 1),

(3, N'Định Luật Murphy', N'Từ Thính Phong',
N'Có phải hằng ngày bạn hay gặp những chuyện dở khóc dở cười tương tự như vậy? Những hiện tượng này đều có thể giải thích bằng một khái niệm tâm lý học thú vị: Định luật Murphy. Nó nhắc nhở chúng ta rằng việc xấu luôn có “cơ may” cao hơn và sai lầm luôn là một phần của thế giới này. Dù cho mỗi chúng ta đều cố gắng hết sức để tránh khỏi sai lầm, nhưng trên thực tế đó là điều bất khả thi.',
119000, 100, N'~/Content/img/dinh-luat-murphy.webp', 1),

(5, N'Chuyên Đề Học Tập Tin Học 12', N'Nhiều Tác Giả',
N'Sách biên soạn theo chương trình giáo dục phổ thông chuẩn. Nội dung các chuyên đề không chỉ kế thừa, phát triển kiến thức trong phần nội dung cơ bản của môn Tin học 12, mà còn tích hợp kiến thức của các môn khác cung cấp cho các em những kiến thức chuyên sâu, bổ ích, lí thú và thiết thực, giúp các em hình thành kĩ năng thực hành, kĩ năng vận dụng kiến thức tin học ứng dụng vào giải quyết vấn đề thực tế.',
16800, 100, N'~/Content/img/sach-tin-hoc-12.webp', 1),

(5, N'Chuyên Đề Học Tập Toán 12', N'Nhiều Tác Giả',
N'Sách biên soạn theo chương trình giáo dục phổ thông chuẩn. Nội dung các chuyên đề không chỉ kế thừa, phát triển kiến thức trong phần nội dung cơ bản của môn Toán 12, mà còn tích hợp kiến thức khác để cung cấp cho các em những kiến thức chuyên sâu, bổ ích, lí thú và thiết thực, giúp các em hình thành kĩ năng thực hành, kĩ năng vận dụng kiến thức đã học ở phần nội dung cơ bản vào giải quyết vấn đề thực tế.',
8800, 100, N'~/Content/img/chuyen-de-hoc-tap-toan-12.webp', 1),

(19, N'Người Thầy', N'Nguyễn Chí Vịnh',
N'Nhân vật trung tâm trong cuốn sách là ông Ba Quốc, tức Thiếu tướng, Anh hùng Lực lượng vũ trang (LLVT) nhân dân Đặng Trần Đức, một nhà tình báo xuất sắc của tình báo quốc phòng Việt Nam, một cán bộ tình báo tài năng, hội tụ đầy đủ những phẩm chất của một điệp viên "chui sâu, leo cao" hoàn hảo, một nhà chỉ huy có tầm nhìn chiến lược, sắc sảo, quyết liệt và là một người thầy có cá tính đặc biệt, nghiêm khắc nhưng vô cùng nhân văn, sâu sắc.',
267000, 100, N'~/Content/img/nguoi-thay.webp', 1),

(16, N'Nếu Biết Trăm Năm Là Hữu Hạn', N'Phạm Lữ Ân',
N'Bạn đã bao giờ tự hỏi: Nếu biết trước cuộc đời là hữu hạn, bạn sẽ sống khác đi chứ?<br><br>Chúng ta luôn nghĩ mình có nhiều thời gian, nhưng thực tế, mọi khoảnh khắc đều đang trôi qua mãi mãi.',
159000, 100, N'~/Content/img/neu-biet-tram-nam-la-huu-han.webp', 1),

(18, N'Sự Trở Về Của Sherlock Holmes', N'Arthur Conan Doyle',
N'Mệt mỏi với danh tiếng nổi trội của vị thám tử tài ba lấn át tất cả các tác phẩm khác của mình, nhà văn Arthur Conan Doyle đã cho anh biến mất trong Hồi ức về Sherlock Holmes. Tuy nhiên, sau lần thỏa hiệp đầu tiên của ông với Con chó của dòng họ Baskerville, được viết dưới dạng hồi tưởng của bác sĩ Watson, nhà thám tử đáng nhớ của căn hộ số 221B phố Baker đã chính thức được hồi sinh trong tập sách Sự trở về của Sherlock Holmes.',
155000, 100, N'~/Content/img/sherlock-holmes-su-tro-ve-cua-sherlock-holmes.webp', 1),

(17, N'Cô Gái Đến Từ Hôm Qua', N'Nguyễn Nhật Ánh',
N'Nếu ngày xưa còn bé, Thư luôn tự hào mình là cậu con trai thông minh có quyền bắt nạt và sai khiến các cô bé cùng lứa tuổi thì giờ đây khi lớn lên, anh luôn khổ sở khi thấy mình ngu ngơ và bị con gái “xỏ mũi”.<br><br>Và điều nghịch lý ấy xem ra càng “trớ trêu’ hơn, khi như một định mệnh, Thư nhận ra Việt An, cô bạn học thông minh thường làm mình bối rối bấy lâu nay chính là Tiểu Li, con bé hàng xóm ngốc nghếch từng hứng chịu những trò nghịch ngợm của mình hồi xưa.',
35000, 100, N'~/Content/img/co-gai-den-tu-hom-qua.webp', 1),

(17, N'Hạ Đỏ', N'Nguyễn Nhật Ánh',
N'Mùa hè năm đó, Chương được bố mẹ cho về quê chơi. Ở đây, cậu được biết đến những trò chơi thú vị cùng hai đứa em con nhà dì của mình, những trò chơi mà Chương chẳng thể kiếm ở đâu được khi còn sống ở thành phố. Rồi Chương gặp Út Thêm và phải lòng cô bé, Chương thích sự nhẹ nhàng ở Út Thêm và chỉ muốn được nhìn thấy cô mỗi ngày. Ngày qua ngày, Chương đã đi qua nhiều cung bậc cảm xúc khi bắt đầu biết rung động trước một người con gái, cậu muốn dành những điều thật vui vẻ và tốt đẹp gửi đến cho cô.<br><br>Chương trở lại thành phố sau những ngày hè nắng cháy, hoa cỏ may trắng muốt mọc đầy trên con đường cậu đi qua mỗi ngày để đến nhà Út Thêm bám đầy gấu quần cậu, như mối tình đầu da diết mà cậu mang trong lòng.',
95000, 100, N'~/Content/img/ha-do.webp', 1),

(1, N'Money Master The Game', N'Tony Robbins',
N'In his first book in two decades, Anthony Robbins turns to the topic that vexes us all: How to secure financial freedom for ourselves and for our families. "If there were a Pulitzer Prize for investment books, this one would win, hands down"',
541000, 100, N'~/Content/img/money-master-the-game.webp', 1),

(16, N'Bến Xe', N'Thương Thái Vi',
N'Bến Xe<br><br>Thứ tôi có thể cho em trong cuộc đời này<br><br>chỉ là danh dự trong sạch<br><br>và một tương lai tươi đẹp mà thôi.<br><br>Thế nhưng, nếu chúng ta có kiếp sau,<br><br>nếu kiếp sau tôi có đôi mắt sáng,<br><br>tôi sẽ ở bến xe này… đợi em.',
76000, 100, N'~/Content/img/ben-xe.webp', 1),

(2, N'Ở Đây Có Niềm Vui - Cửa Hàng Túi Xinh Xắn', N'Fukuzawa Yumiko',
N'Bộ sách “Ở đây có niềm vui” gồm bốn câu chuyện nhỏ đầy ấm áp kể về những nhân vật hiền hòa trong khu rừng luôn dùng lòng tốt, sự thấu hiểu và bàn tay khéo léo để mang lại niềm vui cho người khác. Qua những câu chuyện giản dị, các bạn nhỏ sẽ học về giá trị của sự quan tâm và niềm hạnh phúc khi sẻ chia.',
45000, 100, N'~/Content/img/o-day-co-niem-vui-cua-hang-tui-xinh-xan.jpg', 1),

(2, N'Mẹ Hỏi Bé Trả Lời 1-2 Tuổi', N'Yosbook, Xiao Li',
N'Bộ sách nhỏ xinh “Mẹ hỏi bé trả lời” tập hợp những trò chơi phong phú, câu đố thông minh giúp bé và cha mẹ có thể “học mà chơi, chơi mà học” qua các chủ đề: cách ứng xử, câu hỏi về tự nhiên, không gian, phân biệt hình khối, ngôn ngữ, toán học...<br><br>Sách phân chia theo từng lứa tuổi với nội dung phong phú, những câu hỏi đáp hàm súc trí tuệ, hình ảnh dễ thương cùng nhiều phương pháp rèn luyện, hoàn toàn có thể khơi dậy trí thông minh và khả năng tiềm ẩn của bé.',
30000, 100, N'~/Content/img/me-hoi-be-tra-loi.webp', 1),

(4, N'Ngữ Pháp Tiếng Trung Toàn Diện', N'A Jay Chinese',
N'Tiếng Trung đang nhanh chóng trở thành một trong những ngôn ngữ quan trọng nhất trên thế giới, mang lại cơ hội phát triển vượt bậc trong học tập, nghiên cứu, và sự nghiệp. Tuy nhiên để biến ngôn ngữ này thành công cụ hiệu quả, bạn không nên dừng lại ở việc rèn luyện giao tiếp cơ bản, bạn cần hiểu sâu và nắm vững nền tảng ngôn ngữ Trung, đặc biệt là ngữ pháp!',
115000, 100, N'~/Content/img/ngu-phap-tieng-trung-toan-dien.webp', 1);
GO

-- =============================================
-- 11. CHECK: TABLES CREATED
-- =============================================
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;
GO

-- =============================================
-- 12. CHECK: CATEGORIES
-- =============================================
SELECT *
FROM dbo.Category
ORDER BY CategoryID;
GO

-- =============================================
-- 13. CHECK: 20 BOOKS
-- =============================================
SELECT
    B.BookID,
    B.Title,
    B.AuthorName,
    C.CategoryID,
    C.CategoryName,
    B.Price,
    B.StockQuantity,
    B.ImageURL
FROM dbo.Book AS B
INNER JOIN dbo.Category AS C
    ON B.CategoryID = C.CategoryID
ORDER BY B.BookID;
GO
