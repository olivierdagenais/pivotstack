SELECT
    p.Id AS Id,
    p.Title As Title,
    p.Body AS Description,
    p.Score,
    p.ViewCount AS Views,
    COALESCE (p.AnswerCount, 0) AS Answers,
    p.Tags,
    p.CreationDate AS DateAsked,
    (
        SELECT
            MIN(CreationDate) AS Expr1
        FROM
            Posts AS p2
        WHERE
            (p.Id = ParentId)
    ) AS DateFirstAnswered,
    (
        SELECT
            MAX(CreationDate) AS Expr1
        FROM
            Posts AS p2
        WHERE
            (p.Id = ParentId)
    ) AS DateLastAnswered,
    COALESCE (
        p.OwnerDisplayName,
        (
            /* TODO: not all users have a DisplayName set, such as SuperUser #429; should fetch UserName instead */
            SELECT
                DisplayName
            FROM
                Users AS u1
            WHERE
                (Id = p.OwnerUserId)
        ),
        (
            SELECT
                DisplayName
            FROM
                Users AS u2
            WHERE
                (Id = p.LastEditorUserId)
        )
    ) AS Asker,
    p.AcceptedAnswerId,
    (
        SELECT
            Body
        FROM
            Posts AS p2
        WHERE
            (p.AcceptedAnswerId = Id) AND (PostTypeId = 2)
    ) AS AcceptedAnswer,
    (
        SELECT
            TOP (1) Id
        FROM
            Posts AS p2
        WHERE
            (p.Id = ParentId)
        ORDER BY
            Score DESC
    ) AS TopAnswerId,
    (
        SELECT
            TOP (1) Body
        FROM
            Posts AS p2
        WHERE
            (p.Id = ParentId)
        ORDER BY
            Score DESC
    ) AS TopAnswer,
    COALESCE (p.FavoriteCount, 0) AS Favorites
FROM
    Posts AS p
INNER JOIN
    PostsTags AS pt ON p.Id = pt.PostId
WHERE
    (p.PostTypeId = 1) AND (pt.Tag = @tag)
