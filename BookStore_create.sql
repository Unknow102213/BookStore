/* =====================================================================
   BookStore - Script tạo cơ sở dữ liệu
   ---------------------------------------------------------------------
   - Máy chủ : SQL Server / SQL Server Express (2016 trở lên)
   - Database: BookStore (đúng tên trong Web.config: initial catalog=BookStore)
   - Nội dung: 8 bảng (Category, Users, Book, Cart, CartItem, Orders,
               OrderDetail, Review) + khóa chính/khóa ngoại/UNIQUE/CHECK/
               DEFAULT + toàn bộ dữ liệu hiện có.
   - Cách chạy: mở trong SSMS và bấm Execute (F5) một lần.
   - Lưu ý  : nếu database BookStore đã tồn tại, lệnh CREATE DATABASE sẽ báo
              lỗi và script dừng, không ghi đè dữ liệu cũ. Muốn tạo lại từ
              đầu thì bỏ comment khối DROP ngay bên dưới.
   ===================================================================== */

USE [master];
GO

/*-- Bỏ comment khối này nếu muốn XÓA database cũ rồi tạo lại (mất dữ liệu cũ)
IF DB_ID(N'BookStore') IS NOT NULL
BEGIN
    ALTER DATABASE [BookStore_Test] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [BookStore_Test];
END
GO
*/

CREATE DATABASE [BookStore_Test];
GO

USE [BookStore_Test];
GO

/* =====================================================================
   1. TẠO BẢNG
   ===================================================================== */

-- Category: thể loại sách
CREATE TABLE [dbo].[Category] (
    [CategoryID]   INT IDENTITY(1,1) NOT NULL,
    [CategoryName] NVARCHAR(100)     NOT NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([CategoryID]),
    CONSTRAINT [UQ_Category_CategoryName] UNIQUE ([CategoryName])
);
GO

-- Users: tài khoản (Role: 'A' = Admin, 'C' = Customer)
CREATE TABLE [dbo].[Users] (
    [UserID]       INT IDENTITY(1,1) NOT NULL,
    [Email]        NVARCHAR(255)     NOT NULL,
    [PasswordHash] NVARCHAR(255)     NOT NULL,
    [Role]         CHAR(1)           NOT NULL
        CONSTRAINT [DF_Users_Role] DEFAULT ('C'),
    [IsActive]     BIT               NOT NULL
        CONSTRAINT [DF_Users_IsActive] DEFAULT ((1)),
    [CreatedAt]    DATETIME2(7)      NOT NULL
        CONSTRAINT [DF_Users_CreatedAt] DEFAULT (SYSDATETIME()),
    [FullName]     NVARCHAR(100)     NOT NULL,
    [Phone]        VARCHAR(15)       NULL,
    [Address]      NVARCHAR(500)     NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserID]),
    CONSTRAINT [UQ_Users_Email] UNIQUE ([Email]),
    CONSTRAINT [CK_Users_Role] CHECK ([Role] = 'C' OR [Role] = 'A')
);
GO

-- Book: sách
CREATE TABLE [dbo].[Book] (
    [BookID]        INT IDENTITY(1,1) NOT NULL,
    [CategoryID]    INT               NOT NULL,
    [Title]         NVARCHAR(255)     NOT NULL,
    [AuthorName]    NVARCHAR(500)     NOT NULL,
    [Description]   NVARCHAR(MAX)     NULL,
    [Price]         DECIMAL(18,2)     NOT NULL,
    [StockQuantity] INT               NOT NULL
        CONSTRAINT [DF_Book_StockQuantity] DEFAULT ((0)),
    [ImageURL]      NVARCHAR(500)     NULL,
    [IsActive]      BIT               NOT NULL
        CONSTRAINT [DF_Book_IsActive] DEFAULT ((1)),
    [CreatedAt]     DATETIME2(7)      NOT NULL
        CONSTRAINT [DF_Book_CreatedAt] DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_Book] PRIMARY KEY CLUSTERED ([BookID]),
    CONSTRAINT [FK_Book_Category] FOREIGN KEY ([CategoryID])
        REFERENCES [dbo].[Category] ([CategoryID]),
    CONSTRAINT [CK_Book_Price] CHECK ([Price] >= (0)),
    CONSTRAINT [CK_Book_StockQuantity] CHECK ([StockQuantity] >= (0))
);
GO

-- Cart: mỗi người dùng có tối đa 1 giỏ hàng
CREATE TABLE [dbo].[Cart] (
    [CartID]    INT IDENTITY(1,1) NOT NULL,
    [UserID]    INT               NOT NULL,
    [CreatedAt] DATETIME2(7)      NOT NULL
        CONSTRAINT [DF_Cart_CreatedAt] DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_Cart] PRIMARY KEY CLUSTERED ([CartID]),
    CONSTRAINT [UQ_Cart_UserID] UNIQUE ([UserID]),
    CONSTRAINT [FK_Cart_Users] FOREIGN KEY ([UserID])
        REFERENCES [dbo].[Users] ([UserID])
);
GO

-- CartItem: sách trong giỏ hàng
CREATE TABLE [dbo].[CartItem] (
    [CartItemID] INT IDENTITY(1,1) NOT NULL,
    [CartID]     INT               NOT NULL,
    [BookID]     INT               NOT NULL,
    [Quantity]   INT               NOT NULL,
    CONSTRAINT [PK_CartItem] PRIMARY KEY CLUSTERED ([CartItemID]),
    CONSTRAINT [UQ_CartItem_Cart_Book] UNIQUE ([CartID], [BookID]),
    CONSTRAINT [FK_CartItem_Cart] FOREIGN KEY ([CartID])
        REFERENCES [dbo].[Cart] ([CartID]),
    CONSTRAINT [FK_CartItem_Book] FOREIGN KEY ([BookID])
        REFERENCES [dbo].[Book] ([BookID]),
    CONSTRAINT [CK_CartItem_Quantity] CHECK ([Quantity] > (0))
);
GO

-- Orders: đơn hàng
CREATE TABLE [dbo].[Orders] (
    [OrderID]         INT IDENTITY(1,1) NOT NULL,
    [UserID]          INT               NOT NULL,
    [OrderDate]       DATETIME2(7)      NOT NULL
        CONSTRAINT [DF_Orders_OrderDate] DEFAULT (SYSDATETIME()),
    [TotalAmount]     DECIMAL(18,2)     NOT NULL
        CONSTRAINT [DF_Orders_TotalAmount] DEFAULT ((0)),
    [Status]          NVARCHAR(20)      NOT NULL
        CONSTRAINT [DF_Orders_Status] DEFAULT (N'Preparing'),
    [PaymentMethod]   NVARCHAR(20)      NOT NULL,
    [PaymentStatus]   NVARCHAR(20)      NOT NULL
        CONSTRAINT [DF_Orders_PaymentStatus] DEFAULT (N'Unpaid'),
    [ReceiverName]    NVARCHAR(100)     NOT NULL,
    [ReceiverPhone]   VARCHAR(15)       NOT NULL,
    [AddressDelivery] NVARCHAR(255)     NOT NULL,
    [Notes]           NVARCHAR(500)     NULL,
    [CompletedAt]     DATETIME2(7)      NULL,
    [ReturnDeadline]  DATETIME2(7)      NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([OrderID]),
    CONSTRAINT [FK_Orders_Users] FOREIGN KEY ([UserID])
        REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [CK_Orders_TotalAmount] CHECK ([TotalAmount] >= (0))
);
GO

-- OrderDetail: chi tiết đơn hàng
CREATE TABLE [dbo].[OrderDetail] (
    [OrderDetailID] INT IDENTITY(1,1) NOT NULL,
    [OrderID]       INT               NOT NULL,
    [BookID]        INT               NOT NULL,
    [Quantity]      INT               NOT NULL,
    [UnitPrice]     DECIMAL(18,2)     NOT NULL,
    CONSTRAINT [PK_OrderDetail] PRIMARY KEY CLUSTERED ([OrderDetailID]),
    CONSTRAINT [UQ_OrderDetail_Order_Book] UNIQUE ([OrderID], [BookID]),
    CONSTRAINT [FK_OrderDetail_Orders] FOREIGN KEY ([OrderID])
        REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_OrderDetail_Book] FOREIGN KEY ([BookID])
        REFERENCES [dbo].[Book] ([BookID]),
    CONSTRAINT [CK_OrderDetail_Quantity] CHECK ([Quantity] > (0)),
    CONSTRAINT [CK_OrderDetail_UnitPrice] CHECK ([UnitPrice] >= (0))
);
GO

-- Review: đánh giá sách theo đơn hàng
CREATE TABLE [dbo].[Review] (
    [ReviewID]  INT IDENTITY(1,1) NOT NULL,
    [UserID]    INT               NOT NULL,
    [BookID]    INT               NOT NULL,
    [Rating]    INT               NOT NULL,
    [Comment]   NVARCHAR(1000)    NULL,
    [CreatedAt] DATETIME2(7)      NOT NULL
        CONSTRAINT [DF_Review_CreatedAt] DEFAULT (SYSDATETIME()),
    [OrderID]   INT               NOT NULL,
    CONSTRAINT [PK_Review] PRIMARY KEY CLUSTERED ([ReviewID]),
    CONSTRAINT [UQ_Review_User_Book] UNIQUE ([UserID], [BookID]),
    CONSTRAINT [FK_Review_Users] FOREIGN KEY ([UserID])
        REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_Review_Book] FOREIGN KEY ([BookID])
        REFERENCES [dbo].[Book] ([BookID]),
    CONSTRAINT [FK_Review_Orders] FOREIGN KEY ([OrderID])
        REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [CK_Review_Rating] CHECK ([Rating] >= (1) AND [Rating] <= (5))
);
GO

/* =====================================================================
   2. DỮ LIỆU (chèn theo thứ tự khóa ngoại)
   ===================================================================== */


-- Dữ liệu bảng Category
SET IDENTITY_INSERT [dbo].[Category] ON;
GO
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (7, N'Sách Công Nghệ – Tin Học')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (11, N'Sách Chính Trị – Pháp Luật')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (5, N'Sách Giáo Khoa – Tham Khảo')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (2, N'Sách Kinh Tế')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (8, N'Sách Khoa Học – Đời Sống')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (9, N'Sách Lịch Sử – Địa Lý')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (6, N'Sách Ngoại Ngữ')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (3, N'Sách Tâm Lý – Kỹ Năng Sống')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (15, N'Sách Tôn Giáo – Tâm Linh')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (4, N'Sách Thiếu Nhi')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (10, N'Sách Văn Hóa – Nghệ Thuật')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (18, N'Sách Văn Học Nước Ngoài')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (16, N'Sách Văn Học Việt Nam')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (12, N'Sách Y Học – Sức Khỏe')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (14, N'Từ Điển')
INSERT [dbo].[Category] ([CategoryID], [CategoryName]) VALUES (13, N'Truyện Tranh')
GO
SET IDENTITY_INSERT [dbo].[Category] OFF;
GO


-- Dữ liệu bảng Users
SET IDENTITY_INSERT [dbo].[Users] ON;
GO
INSERT [dbo].[Users] ([UserID], [Email], [PasswordHash], [Role], [IsActive], [CreatedAt], [FullName], [Phone], [Address]) VALUES (1, N'trantuanvu213@gmail.com', N'$2a$11$PGT2qMJfv0J836hfLgWAJeCYXjImi5aDK0U8ko0F41/e01wKsBWJ2', N'A', 1, CAST(N'2026-10-25T14:29:39.0227393' AS DateTime2), N'Vũ Trần', N'0981092793', N'31 Ngô Quyền, Phường 1 Bảo Lộc, Lâm Đồng')
INSERT [dbo].[Users] ([UserID], [Email], [PasswordHash], [Role], [IsActive], [CreatedAt], [FullName], [Phone], [Address]) VALUES (4, N'trantuanvu102213@gmail.com', N'$2a$11$Lsf8RljzdDM699LwKiVQXOCyBBrc0apXaN8J00eK8RQJGAKjjf8SW', N'C', 1, CAST(N'2026-11-15T23:37:12.3611324' AS DateTime2), N'Tuấn Vũ', NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Users] OFF;
GO


-- Dữ liệu bảng Book
SET IDENTITY_INSERT [dbo].[Book] ON;
GO
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (1, 3, N'Giá Trị Của Sự Tử Tế', N'Piero Ferrucci', N'Lòng tốt và sự đồng cảm là hai trong số nhiều điều thiết yếu khiến cuộc sống của chúng ta có ý nghĩa. Chúng là khởi nguồn cho hạnh phúc và niềm vui vĩnh cửu. Chúng là nền tảng của một trái tim nồng ấm, trái tim của người sẵn lòng dang tay giúp đỡ người khác một cách vô tư. Bằng lòng tốt, tình thương yêu, sự trung thực, chân thành và công bằng với tất cả mọi người, ta có thể bảo vệ sự tử tế. Đó là lẽ thường tình.

Quan tâm tới mọi người là một việc đáng làm, bởi vì hạnh phúc của chúng ta gắn liền với hạnh phúc của những người khác. Nếu xã hội chìm trong đau khổ, bản thân chúng ta cũng đau khổ. Khi trái tim và tâm trí ta càng bị cái ác lấn át, ta càng trở nên bất hạnh. Bởi vậy, lòng tốt và sự đồng cảm là những giá trị ta không thể thiếu được.

Việc cố gắng tạo ra điều gì đó tích cực trong cuộc sống là điều vô cùng quan trọng. Chúng ta không được sinh ra để gây rắc rối và làm hại người khác. Như Piero Ferrucci đã trình bày một cách đầy đủ và mạch lạc trong cuốn sách Giá trị của sự tử tế này, để cuộc sống thật sự có giá trị, ta cần củng cố và nuôi dưỡng những giá trị cơ bản của con người như sự nhiệt thành, lòng tốt và sự đồng cảm. Nếu làm được điều đó, cuộc sống của chúng ta sẽ trở nên có ý nghĩa, hạnh phúc và bình an hơn, đồng thời sẽ góp một phần tích cực vào thế giới quanh mình.', CAST(120000.00 AS Decimal(18, 2)), 100, N'~/Content/img/f53347cd-365c-456b-8f09-aa044b8cae52.webp', 1, CAST(N'2026-10-25T21:58:38.4095201' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (2, 16, N'Than Ấm Bừng Lên', N'Lam', N'Đôi khi, giữa những bộn bề hôm nay, ta chỉ muốn chạm vào chút ấm áp xưa cũ - như hơi than hồng trong một chiều gió lạnh, như tiếng bà gọi vọng từ căn bếp nhỏ. “Than Ấm Bừng Lên” của tác giả Lam đưa ta trở lại nơi ấy, nhẹ nhàng mà trọn vẹn.
Nếu như ""Trốn lên mái nhà để khóc"" đã từng chạm đến trái tim HÀNG VẠN ĐỘC GIẢ bằng cách tiếp thêm cho chúng ta can đảm để đối mặt với những tổn thương, tìm lại bình yên cho tâm hồn và “Thưa ngoại con mới về” là bức thư dài chan chứa nỗi nhớ thương dành cho ngoại và mái nhà quê yêu dấu, thì “Than ấm bừng lên” lại giống như một bếp lửa hồng êm dịu, sưởi ấm cả một miền ký ức tuổi thơ.
Tác phẩm tập hợp những bài thơ nhỏ, giản dị nhưng thấm đẫm tình cảm: buổi chiều mùa đông bà nhóm bếp than, mùi khói len qua giàn mướp chín gọi chân trẻ về; buổi tan học được ông đón trên con đường vàng nắng; nồi canh thơm nghi ngút mùi quê nhà; những mùa quả chín đợi người xa trở lại… Mỗi bài thơ là một khung ảnh chan chứa hơi ấm, khắc họa hình ảnh ông bà, cha mẹ và cả những góc vườn nhà, con vật… tất cả tạo thành bức tranh làng quê mộc mạc nhưng sống động. Những trang sách như một bếp than hồng, vừa ấm áp, vừa lung linh ánh sáng của yêu thương. Ta thấy ở đó tiếng lòng của những đứa cháu đã đi xa, mang trong mình nỗi nhớ khôn nguôi về hơi ấm và tình thương của bà:

“Bà ơi, con chẳng hứa suông
Chỉ là ngọn khói nó buồn, nó đau
Nó không nhóm bếp cho mau
Nó ngồi thút thít ở sau hiên nhà
Nó làm con cũng nhớ bà của con…”

Điều đặc biệt là Lam đã khéo léo đưa vào những bài học nhẹ nhàng về yêu thương, trân trọng, và gìn giữ những giá trị tốt đẹp. Lời thơ giản dị, giàu hình ảnh, đôi lúc hồn nhiên như tiếng trẻ con kể chuyện, đôi lúc lại trầm lắng như lời bà thủ thỉ bên bếp lửa.
Tập thơ sẽ dành cho bạn trong những chiều thư thả, để đọc chậm, để nhâm nhi từng câu chữ và để nhớ rằng hạnh phúc đôi khi nằm gọn trong những điều nhỏ bé nhất.
Khép lại “Than ấm bừng lên”, ta như vừa ngồi bên bếp lửa của tuổi thơ để hơi ấm len qua từng kẽ tay, để mùi khói vương trên vạt áo. Và khi bước ra đời, ta vẫn mang theo ngọn lửa ấy - một ngọn lửa của yêu thương, để tự sưởi ấm mình và để ủ ấm những người mà ta yêu thương.', CAST(105000.00 AS Decimal(18, 2)), 100, N'\Content\img\than-am-bung-len.webp', 1, CAST(N'2026-10-25T22:22:15.4657854' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (3, 16, N'Dear, Người Yêu Cũ ', N'Cammpodcast, Đỗ Quỳnh', N'"DEAR, NGƯỜI YÊU CŨ – Cuốn sách dành cho những trái tim đang học cách buông tay.
Nếu bạn từng có một người khiến mình mất rất lâu để quên, từng tự hỏi “Tại sao họ có thể bước tiếp còn mình thì vẫn mắc kẹt?”, Dear, người yêu cũ có lẽ sẽ chạm đến một góc rất quen thuộc trong bạn."', CAST(86000.00 AS Decimal(18, 2)), 100, N'\Content\img\dear-nguoi-yeu-cu.webp', 1, CAST(N'2026-10-25T22:34:01.1885761' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (4, 16, N'Số Đỏ', N'Vũ Trọng Phụng', N'- Tôi không muốn lấy cái người ấy, vì nếu tôi lấy hắn, chắc hắn sẽ mọc sừng. Thí dụ một người như ông mà hỏi tôi thì còn nói gì! Đằng này hắn lại nhà quê, không biết yêu vợ như những người văn minh. Buồn lắm anh ạ.
- Tôi phải làm gì?
- Phải giả vờ chim tôi..., chúng ta giả vờ chim nhau, mê nhau... Cho hắn bỏ tôi, mình hiểu chưa? Ta giả vờ với nhau thôi mà! Tôi cần mang tiếng hư hỏng lắm mới được.
- Thế sao nữa ạ?
- Anh thì anh cũng phải mang tiếng là làm hại một đời tôi mới xong!
', CAST(98000.00 AS Decimal(18, 2)), 100, N'\Content\img\so-do.webp', 1, CAST(N'2026-10-25T22:36:27.8443724' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (5, 2, N'Doanh Nghiệp Tự Hành', N'Mike Michalowicz', N'Sách về cách xây dựng doanh nghiệp có thể tự vận hành và phát triển.', CAST(139000.00 AS Decimal(18, 2)), 99, N'~/Content/img/clock-work.webp', 1, CAST(N'2026-10-25T22:50:33.1533333' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (6, 2, N'Lãnh Đạo Bằng Sức Mạnh Trí Tuệ Cảm Xúc', N'Daniel Goleman, Richard Boyatzis, Annie Mckee', N'Sách về trí tuệ cảm xúc, năng lực lãnh đạo và truyền cảm hứng.', CAST(148000.00 AS Decimal(18, 2)), 98, N'~/Content/img/lanh-dao-bang-suc-manh-va-tri-tue-cam-xuc.webp', 1, CAST(N'2026-10-25T22:50:33.1533333' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (7, 2, N'Money Master The Game', N'Tony Robbins', N'Sách về tự do tài chính, đầu tư và quản lý tài chính cá nhân.', CAST(541000.00 AS Decimal(18, 2)), 98, N'~/Content/img/money-master-the-game.webp', 1, CAST(N'2026-10-25T22:50:33.1533333' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (8, 4, N'Ở Đây Có Niềm Vui - Cửa Hàng Túi Xinh Xắn', N'Fukuzawa Yumiko', N'Câu chuyện thiếu nhi ấm áp về lòng tốt và sự sẻ chia.', CAST(45000.00 AS Decimal(18, 2)), 89, N'~/Content/img/o-day-co-niem-vui-cua-hang-tui-xinh-xan.jpg', 1, CAST(N'2026-10-25T22:50:33.1533333' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (9, 4, N'Mẹ Hỏi Bé Trả Lời 1-2 Tuổi', N'Yosbook, Xiao Li', N'Sách tương tác giúp trẻ nhỏ học qua câu hỏi, trò chơi và hình ảnh.', CAST(30000.00 AS Decimal(18, 2)), 0, N'~/Content/img/me-hoi-be-tra-loi.webp', 1, CAST(N'2026-10-25T22:50:33.1533333' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (10, 6, N'Ngữ Pháp Tiếng Trung Toàn Diện', N'A Jay Chinese', N'Sách học ngữ pháp tiếng Trung và củng cố nền tảng ngôn ngữ.', CAST(115000.00 AS Decimal(18, 2)), 100, N'~/Content/img/ngu-phap-tieng-trung-toan-dien.webp', 1, CAST(N'2026-10-25T22:50:33.1533333' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (11, 3, N'Thiên Tài Bên Trái, Kẻ Điên Bên Phải', N'Cao Minh', N'Sách tâm lý giúp khám phá cách suy nghĩ và nhìn nhận thế giới.', CAST(179000.00 AS Decimal(18, 2)), 99, N'~/Content/img/thien-tai-ben-trai-ke-dien-ben-phai.webp', 1, CAST(N'2026-10-25T22:57:48.4600000' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (12, 3, N'Định Luật Murphy', N'Từ Thính Phong', N'Những góc nhìn về tâm lý và các tình huống thường gặp trong cuộc sống.', CAST(119000.00 AS Decimal(18, 2)), 100, N'~/Content/img/dinh-luat-murphy.webp', 1, CAST(N'2026-10-25T22:57:48.4600000' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (13, 5, N'Chuyên Đề Học Tập Tin Học 12', N'Nhiều Tác Giả', N'Sách chuyên đề học tập môn Tin học lớp 12.', CAST(16800.00 AS Decimal(18, 2)), 100, N'~/Content/img/sach-tin-hoc-12.webp', 1, CAST(N'2026-10-25T22:57:48.4600000' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (14, 5, N'Chuyên Đề Học Tập Toán 12', N'Nhiều Tác Giả', N'Sách chuyên đề học tập môn Toán lớp 12.', CAST(8800.00 AS Decimal(18, 2)), 100, N'~/Content/img/chuyen-de-hoc-tap-toan-12.webp', 1, CAST(N'2026-10-25T22:57:48.4600000' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (15, 16, N'Người Thầy', N'Nguyễn Chí Vịnh', N'Câu chuyện về người thầy và những trải nghiệm trong cuộc đời.', CAST(267000.00 AS Decimal(18, 2)), 98, N'~/Content/img/nguoi-thay.webp', 1, CAST(N'2026-10-25T22:57:48.4600000' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (16, 16, N'Nếu Biết Trăm Năm Là Hữu Hạn', N'Phạm Lữ Ân', N'Những suy ngẫm về cuộc sống, tình yêu và tuổi trẻ.', CAST(159000.00 AS Decimal(18, 2)), 98, N'~/Content/img/neu-biet-tram-nam-la-huu-han.webp', 1, CAST(N'2026-10-25T22:57:48.4600000' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (17, 18, N'Sự Trở Về Của Sherlock Holmes', N'Arthur Conan Doyle', N'Tập truyện trinh thám về thám tử Sherlock Holmes.', CAST(155000.00 AS Decimal(18, 2)), 96, N'~/Content/img/sherlock-holmes-su-tro-ve-cua-sherlock-holmes.webp', 1, CAST(N'2026-10-25T22:57:48.4600000' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (18, 16, N'Cô Gái Đến Từ Hôm Qua', N'Nguyễn Nhật Ánh', N'Câu chuyện về tình bạn, tuổi học trò và những ký ức tuổi thơ.', CAST(35000.00 AS Decimal(18, 2)), 99, N'~/Content/img/co-gai-den-tu-hom-qua.webp', 1, CAST(N'2026-10-25T22:57:48.4600000' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (19, 16, N'Hạ Đỏ', N'Nguyễn Nhật Ánh', N'Câu chuyện về tình cảm và những kỷ niệm của tuổi học trò.', CAST(95000.00 AS Decimal(18, 2)), 98, N'~/Content/img/ha-do.webp', 1, CAST(N'2026-10-25T22:57:48.4600000' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (20, 16, N'Bến Xe', N'Thương Thái Vi', N'Câu chuyện tình cảm với những lựa chọn và thử thách trong cuộc sống.', CAST(76000.00 AS Decimal(18, 2)), 98, N'~/Content/img/ben-xe.webp', 1, CAST(N'2026-10-25T22:57:48.4600000' AS DateTime2))
INSERT [dbo].[Book] ([BookID], [CategoryID], [Title], [AuthorName], [Description], [Price], [StockQuantity], [ImageURL], [IsActive], [CreatedAt]) VALUES (22, 2, N'48 Nguyên Tắc Chủ Chốt Của Quyền Lực', N'Robert Greene', N'Phi luân lý, xảo quyệt, nhẫn tâm và dồi dào tư liệu, “48 nguyên tắc chủ chốt của quyền lực” của Robert Greene hoàn toàn có thể giúp bạn vươn tới những đỉnh cao quyền lực và cũng có thể giúp bạn đạt được tột đỉnh vinh quang.', CAST(144000.00 AS Decimal(18, 2)), 100, N'~/Content/img/b897a4e6-6b97-40af-afd1-0ca34089f8f0.jpg', 1, CAST(N'2026-11-23T20:30:26.9399824' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[Book] OFF;
GO


-- Dữ liệu bảng Cart
SET IDENTITY_INSERT [dbo].[Cart] ON;
GO
INSERT [dbo].[Cart] ([CartID], [UserID], [CreatedAt]) VALUES (1, 1, CAST(N'2026-10-26T21:05:34.5441415' AS DateTime2))
INSERT [dbo].[Cart] ([CartID], [UserID], [CreatedAt]) VALUES (3, 4, CAST(N'2026-11-16T00:32:48.2775341' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[Cart] OFF;
GO


-- Dữ liệu bảng CartItem
SET IDENTITY_INSERT [dbo].[CartItem] ON;
GO
INSERT [dbo].[CartItem] ([CartItemID], [CartID], [BookID], [Quantity]) VALUES (34, 3, 5, 1)
INSERT [dbo].[CartItem] ([CartItemID], [CartID], [BookID], [Quantity]) VALUES (35, 3, 18, 1)
INSERT [dbo].[CartItem] ([CartItemID], [CartID], [BookID], [Quantity]) VALUES (36, 3, 17, 1)
GO
SET IDENTITY_INSERT [dbo].[CartItem] OFF;
GO


-- Dữ liệu bảng Orders
SET IDENTITY_INSERT [dbo].[Orders] ON;
GO
INSERT [dbo].[Orders] ([OrderID], [UserID], [OrderDate], [TotalAmount], [Status], [PaymentMethod], [PaymentStatus], [ReceiverName], [ReceiverPhone], [AddressDelivery], [Notes], [CompletedAt], [ReturnDeadline]) VALUES (1, 1, CAST(N'2026-11-01T21:07:12.2316951' AS DateTime2), CAST(693000.00 AS Decimal(18, 2)), N'Đã nhận hàng', N'COD', N'Chưa thanh toán', N'Vũ Trần', N'0981092793', N'31 Ngô Quyền, Phường 1 Bảo Lộc, Lâm Đồng', NULL, CAST(N'2026-11-01T22:54:24.4722785' AS DateTime2), CAST(N'2026-11-08T22:54:24.4722785' AS DateTime2))
INSERT [dbo].[Orders] ([OrderID], [UserID], [OrderDate], [TotalAmount], [Status], [PaymentMethod], [PaymentStatus], [ReceiverName], [ReceiverPhone], [AddressDelivery], [Notes], [CompletedAt], [ReturnDeadline]) VALUES (2, 1, CAST(N'2026-11-01T21:10:36.8100515' AS DateTime2), CAST(853000.00 AS Decimal(18, 2)), N'Đang chuẩn bị', N'COD', N'Chưa thanh toán', N'Vũ Trần', N'0981092793', N'31 Ngô Quyền, Phường 1 Bảo Lộc, Lâm Đồng', NULL, NULL, NULL)
INSERT [dbo].[Orders] ([OrderID], [UserID], [OrderDate], [TotalAmount], [Status], [PaymentMethod], [PaymentStatus], [ReceiverName], [ReceiverPhone], [AddressDelivery], [Notes], [CompletedAt], [ReturnDeadline]) VALUES (3, 1, CAST(N'2026-11-01T21:31:59.0431840' AS DateTime2), CAST(318000.00 AS Decimal(18, 2)), N'Chờ xử lý', N'COD', N'Chưa thanh toán', N'Vũ Trần', N'0981092793', N'31 Ngô Quyền, Phường 1 Bảo Lộc, Lâm Đồng', NULL, NULL, NULL)
INSERT [dbo].[Orders] ([OrderID], [UserID], [OrderDate], [TotalAmount], [Status], [PaymentMethod], [PaymentStatus], [ReceiverName], [ReceiverPhone], [AddressDelivery], [Notes], [CompletedAt], [ReturnDeadline]) VALUES (4, 1, CAST(N'2026-11-01T21:41:36.9808997' AS DateTime2), CAST(689000.00 AS Decimal(18, 2)), N'Đã hủy', N'COD', N'Chưa thanh toán', N'Vũ Trần', N'0981092793', N'31 Ngô Quyền, Phường 1 Bảo Lộc, Lâm Đồng', NULL, NULL, NULL)
INSERT [dbo].[Orders] ([OrderID], [UserID], [OrderDate], [TotalAmount], [Status], [PaymentMethod], [PaymentStatus], [ReceiverName], [ReceiverPhone], [AddressDelivery], [Notes], [CompletedAt], [ReturnDeadline]) VALUES (7, 4, CAST(N'2026-11-16T01:12:35.9010769' AS DateTime2), CAST(190000.00 AS Decimal(18, 2)), N'Đã nhận hàng', N'COD', N'Đã thanh toán', N'Tuấn Vũ', N'0981092793', N'fdsbgfnhdgmjrkgshdfgkltureyrtrafebncm,kgltirueytrgshdfjktltiuytgrnhmfjktikuytgrfbgnmfjktuytgfdsbgfnhdgmjrkgshdfgkltureyrtrafebncm,kgltirueytrgshdfjktltiuytgrnhmfjktikuytgrfbgnmfjktuytg', N'fdsbgfnhdgmjrkgshdfgkltureyrtrafebncm,kgltirueytrgshdfjktltiuytgrnhmfjktikuytgrfbgnmfjktuytgfdsbgfnhdgmjrkgshdfgkltureyrtrafebncm,kgltirueytrgshdfjktltiuytgrnhmfjktikuytgrfbgnmfjktuytg', NULL, NULL)
INSERT [dbo].[Orders] ([OrderID], [UserID], [OrderDate], [TotalAmount], [Status], [PaymentMethod], [PaymentStatus], [ReceiverName], [ReceiverPhone], [AddressDelivery], [Notes], [CompletedAt], [ReturnDeadline]) VALUES (8, 4, CAST(N'2026-11-16T01:28:29.3860650' AS DateTime2), CAST(190000.00 AS Decimal(18, 2)), N'Đã nhận hàng', N'COD', N'Đã thanh toán', N'Tuấn Vũ', N'0987654321', N'aghsfmj,ktltktuytgsbdnhmjktlkytrj,kylluytegbnhgmfjktliuytgbdnhgmfjkukuytgbgdnmgfjktytggdnmgfhgkujyhtgfgndm,kkhygtggk,jh', N'aghsfmj,ktltktuytgsbdnhmjktlkytrj,kylluytegbnhgmfjktliuytgbdnhgmfjkukuytgbgdnmgfjktytggdnmgfhgkujyhtgfgndm,kkhygtggk,jh', CAST(N'2026-11-16T03:07:21.1645425' AS DateTime2), CAST(N'2026-11-23T03:07:21.1645425' AS DateTime2))
INSERT [dbo].[Orders] ([OrderID], [UserID], [OrderDate], [TotalAmount], [Status], [PaymentMethod], [PaymentStatus], [ReceiverName], [ReceiverPhone], [AddressDelivery], [Notes], [CompletedAt], [ReturnDeadline]) VALUES (9, 4, CAST(N'2026-11-16T01:47:07.8415009' AS DateTime2), CAST(314000.00 AS Decimal(18, 2)), N'Đã nhận hàng', N'COD', N'Đã thanh toán', N'Tuấn Vũ', N'0988006401', N'fadhlkjfdsahlkjfdsahlfdsahlkfdsahlkjfdsahkjfdshkjdsfhkjdfshlkjdsfhkjdsfhkjdfshjkfadhlkjfdsahlkjfdsahlfdsahlkfdsahlkjfdsahkjfdshkjdsfhkjdfshlkjdsfhkjdsfhkjdfshjkfadhlkjfdsahlkjfdsahlfdsahlkfdsahlkjfdsahkjfdshkjdsfhkjdfshlkjdsfhkjdsfhkjdfshjkfadhlkjfdsahlkj', N'fadhlkjfdsahlkjfdsahlfdsahlkfdsahlkjfdsahkjfdshkjdsfhkjdfshlkjdsfhkjdsfhkjdfshjkfadhlkjfdsahlkjfdsahlfdsahlkfdsahlkjfdsahkjfdshkjdsfhkjdfshlkjdsfhkjdsfhkjdfshjkfadhlkjfdsahlkjfdsahlfdsahlkfdsahlkjfdsahkjfdshkjdsfhkjdfshlkjdsfhkjdsfhkjdfshjkfadhlkjfdsahlkjfdsahlfdsahlkfdsahlkjfdsahkjfdshkjdsfhkjdfshlkjdsfhkjdsfhkjdfshjkfadhlkjfdsahlkjfdsahlfdsahlkfdsahlkjfdsahkjfdshkjdsfhkjdfshlkjdsfhkjdsfhkjdfshjkfadhlkjfdsahlkjfdsahlfdsahlkfdsahlkjfdsahkjfdshkjdsfhkjdfshlkjdsfhkjdsfhkjdfshjkfadhlkjfdsahlkjfdsah', CAST(N'2026-11-16T03:16:06.8819821' AS DateTime2), CAST(N'2026-11-23T03:16:06.8819821' AS DateTime2))
INSERT [dbo].[Orders] ([OrderID], [UserID], [OrderDate], [TotalAmount], [Status], [PaymentMethod], [PaymentStatus], [ReceiverName], [ReceiverPhone], [AddressDelivery], [Notes], [CompletedAt], [ReturnDeadline]) VALUES (10, 4, CAST(N'2026-11-16T03:24:59.8921621' AS DateTime2), CAST(343000.00 AS Decimal(18, 2)), N'Đã nhận hàng', N'COD', N'Đã thanh toán', N'Tuấn Vũ', N'0987654321', N'davsdbfngmhjlykutyrerwedascvxbcnvmhj,kjy', N'dsafgfnmgjky6543eqwadsfgjhluo86ueyrwteadszvbnmjhku', CAST(N'2026-11-16T03:25:46.6603792' AS DateTime2), CAST(N'2026-11-23T03:25:46.6603792' AS DateTime2))
INSERT [dbo].[Orders] ([OrderID], [UserID], [OrderDate], [TotalAmount], [Status], [PaymentMethod], [PaymentStatus], [ReceiverName], [ReceiverPhone], [AddressDelivery], [Notes], [CompletedAt], [ReturnDeadline]) VALUES (11, 4, CAST(N'2026-11-16T03:32:50.6552049' AS DateTime2), CAST(689000.00 AS Decimal(18, 2)), N'Đã nhận hàng', N'COD', N'Đã thanh toán', N'Tuấn Vũ', N'0987654321', N'avfsbgfhnmj,kyl.;olykmjyhngbvfavfsbgfhnmj,kyl.;olykmjyhngbvfavfsbgfhnmj,kyl.;olykmjyhngbvfavfsbgfhnmj,kyl.;olykmjyhngbvf', N'avfsbgfhnmj,kyl.;olykmjyhngbvfavfsbgfhnmj,kyl.;olykmjyhngbvfavfsbgfhnmj,kyl.;olykmjyhngbvfavfsbgfhnmj,kyl.;olykmjyhngbvf', CAST(N'2026-11-16T03:33:35.1005781' AS DateTime2), CAST(N'2026-11-23T03:33:35.1005781' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[Orders] OFF;
GO


-- Dữ liệu bảng OrderDetail
SET IDENTITY_INSERT [dbo].[OrderDetail] ON;
GO
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (1, 1, 20, 1, CAST(76000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (2, 1, 16, 1, CAST(159000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (3, 1, 17, 2, CAST(155000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (4, 1, 6, 1, CAST(148000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (5, 2, 8, 1, CAST(45000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (6, 2, 15, 1, CAST(267000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (7, 2, 7, 1, CAST(541000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (8, 3, 11, 1, CAST(179000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (9, 3, 5, 1, CAST(139000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (10, 4, 6, 1, CAST(148000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (11, 4, 7, 1, CAST(541000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (14, 7, 18, 1, CAST(35000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (15, 7, 17, 1, CAST(155000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (16, 8, 19, 2, CAST(95000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (17, 9, 17, 1, CAST(155000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (18, 9, 16, 1, CAST(159000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (19, 10, 20, 1, CAST(76000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (20, 10, 15, 1, CAST(267000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (21, 11, 6, 1, CAST(148000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderDetail] ([OrderDetailID], [OrderID], [BookID], [Quantity], [UnitPrice]) VALUES (22, 11, 7, 1, CAST(541000.00 AS Decimal(18, 2)))
GO
SET IDENTITY_INSERT [dbo].[OrderDetail] OFF;
GO


-- Dữ liệu bảng Review
SET IDENTITY_INSERT [dbo].[Review] ON;
GO
INSERT [dbo].[Review] ([ReviewID], [UserID], [BookID], [Rating], [Comment], [CreatedAt], [OrderID]) VALUES (1, 4, 6, 5, N'sách rất hay.', CAST(N'2026-11-16T06:30:37.1887592' AS DateTime2), 11)
INSERT [dbo].[Review] ([ReviewID], [UserID], [BookID], [Rating], [Comment], [CreatedAt], [OrderID]) VALUES (2, 4, 7, 5, N'sách rất hay.', CAST(N'2026-11-16T06:30:37.3255970' AS DateTime2), 11)
GO
SET IDENTITY_INSERT [dbo].[Review] OFF;
GO


/* =====================================================================
   3. KIỂM TRA NHANH: số dòng của từng bảng
   ===================================================================== */
SELECT N'Category'    AS [Bảng], COUNT(*) AS [Số dòng] FROM [dbo].[Category]
UNION ALL SELECT N'Users',       COUNT(*) FROM [dbo].[Users]
UNION ALL SELECT N'Book',        COUNT(*) FROM [dbo].[Book]
UNION ALL SELECT N'Cart',        COUNT(*) FROM [dbo].[Cart]
UNION ALL SELECT N'CartItem',    COUNT(*) FROM [dbo].[CartItem]
UNION ALL SELECT N'Orders',      COUNT(*) FROM [dbo].[Orders]
UNION ALL SELECT N'OrderDetail', COUNT(*) FROM [dbo].[OrderDetail]
UNION ALL SELECT N'Review',      COUNT(*) FROM [dbo].[Review];
GO
