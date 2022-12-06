using TexturePacker.Components;

namespace TexturePacker
{
    internal static unsafe class PackerMethods
    {
        public static void InitTarget(Context* context, int width, int height, Node* nodes, int numNodes)
        {
            int i;
            for (i = 0; i < numNodes - 1; i++)
            {
                nodes[i].Next = &nodes[i + 1];
            }

            nodes[i].Next = null;
            context->Heuristic = Enums.HeuristicSkylineType.BLSortHeight;
            context->FreeHead = &nodes[0];
            context->ActiveHead = &context->Extra[0];
            context->Width = width;
            context->Height = height;
            context->NumNodes = numNodes;

            ConfigOutOfMem(context, false);

            context->Extra[0].X = 0;
            context->Extra[0].Y = 0;
            context->Extra[0].Next = &context->Extra[1];
            context->Extra[1].X = width;
            context->Extra[1].Y = 65535;
            context->Extra[1].Next = null;
        }

        public static int PackRects(Context* context, Rect* rects, int numRects)
        {
            int i = 0;
            int allRectsPacked = 1;

            for (i = 0; i < numRects; ++i)
            {
                rects[i].WasPacked = i;
            }

            CRuntime.QSort(rects, numRects, &CompareRectHeight);

            for (i = 0; i < numRects; ++i)
            {
                ref Rect rect = ref rects[i];

                if (rect.W == 0 || rect.H == 0)
                {
                    rect.X = rect.Y = 0;
                }
                else
                {
                    FindResult fr = PackSkylineRectangle(context, rect.W, rect.H);

                    if (fr.PrevLink != null)
                    {
                        rect.X = fr.X;
                        rect.Y = fr.Y;
                    }
                    else
                    {
                        rect.X = rect.Y = 0xffff;
                    }
                }
            }

            CRuntime.QSort(rects, numRects, &GetRectOriginalOrder);

            for (i = 0; i < numRects; ++i)
            {
                ref Rect rect = ref rects[i];

                rect.WasPacked = rect.X == 0xffff && rect.Y == 0xffff ? 0 : 1;

                if (rect.WasPacked == 0)
                    allRectsPacked = 0;
            }

            return allRectsPacked;
        }

        private static void ConfigOutOfMem(Context* context, bool allowOutOfMem)
        {
            if (allowOutOfMem)
                context->Align = 1;
            else
                context->Align = (context->Width + context->NumNodes - 1) / context->NumNodes;
        }

        private static int FindSkylineMinY(Context* c, Node* first, int x0, int width, int* pwaste)
        {
            Node* node = first;
            int x1 = x0 + width;
            int minY = 0;
            int visitedWidth = 0;
            int wasteArea = 0;

            while (node->X < x1)
            {
                if (node->Y > minY)
                {
                    wasteArea += visitedWidth * (node->Y - minY);
                    minY = node->Y;

                    if (node->X < x0)
                        visitedWidth += node->Next->X - x0;
                    else
                        visitedWidth += node->Next->X - node->X;
                }
                else
                {
                    int under_width = node->Next->X - node->X;
                    if (under_width + visitedWidth > width)
                        under_width = width - visitedWidth;

                    wasteArea += under_width * (minY - node->Y);
                    visitedWidth += under_width;
                }

                node = node->Next;
            }

            *pwaste = wasteArea;
            return minY;
        }

        private static FindResult FindSkylineBestPos(Context* c, int width, int height)
        {
            int bestWaste = 1 << 30;
            int bestX = 0;
            int bestY = 1 << 30;
            FindResult fr = new();
            Node** prev;
            Node* node;
            Node* tail;
            Node** best = null;

            width = width + c->Align - 1;
            width -= width % c->Align;

            if (width > c->Width || height > c->Height)
            {
                fr.PrevLink = null;
                fr.X = fr.Y = 0;
                return fr;
            }

            node = c->ActiveHead;
            prev = &c->ActiveHead;

            while (node->X + width <= c->Width)
            {
                int y = 0;
                int waste = 0;
                y = FindSkylineMinY(c, node, node->X, width, &waste);

                if (c->Heuristic == Enums.HeuristicSkylineType.BLSortHeight)
                {
                    if (y < bestY)
                    {
                        bestY = y;
                        best = prev;
                    }
                }
                else
                {
                    if (y + height <= c->Height && (y < bestY || y == bestY && waste < bestWaste))
                    {
                        bestY = y;
                        bestWaste = waste;
                        best = prev;
                    }
                }

                prev = &node->Next;
                node = node->Next;
            }

            bestX = best == null ? 0 : (*best)->X;

            if (c->Heuristic == Enums.HeuristicSkylineType.BFSortHeight)
            {
                tail = c->ActiveHead;
                node = c->ActiveHead;
                prev = &c->ActiveHead;

                while (tail->X < width)
                {
                    tail = tail->Next;
                }

                while (tail != null)
                {
                    int xpos = tail->X - width;
                    int y = 0;
                    int waste = 0;

                    while (node->Next->X <= xpos)
                    {
                        prev = &node->Next;
                        node = node->Next;
                    }

                    y = FindSkylineMinY(c, node, xpos, width, &waste);

                    if (y + height <= c->Height && y <= bestY && (y < bestY || waste < bestWaste || waste == bestWaste && xpos < bestX))
                    {
                        bestX = xpos;
                        bestY = y;
                        bestWaste = waste;
                        best = prev;
                    }

                    tail = tail->Next;
                }
            }

            fr.PrevLink = best;
            fr.X = bestX;
            fr.Y = bestY;
            return fr;
        }

        private static FindResult PackSkylineRectangle(Context* context, int width, int height)
        {
            FindResult res = FindSkylineBestPos(context, width, height);
            Node* node;
            Node* cur;

            if (res.PrevLink == null || res.Y + height > context->Height || context->FreeHead == null)
            {
                res.PrevLink = null;
                return res;
            }

            node = context->FreeHead;
            node->X = res.X;
            node->Y = res.Y + height;
            context->FreeHead = node->Next;
            cur = *res.PrevLink;

            if (cur->X < res.X)
            {
                Node* next = cur->Next;
                cur->Next = node;
                cur = next;
            }
            else
            {
                *res.PrevLink = node;
            }

            while (cur->Next != null && cur->Next->X <= res.X + width)
            {
                Node* next = cur->Next;
                cur->Next = context->FreeHead;
                context->FreeHead = cur;
                cur = next;
            }

            node->Next = cur;

            if (cur->X < res.X + width)
                cur->X = res.X + width;

            return res;
        }

        private static int CompareRectHeight(Rect* a, Rect* b)
        {
            if (a->H > b->H)
                return -1;

            if (a->H < b->H)
                return 1;

            return a->W > b->W ? -1 : a->W < b->W ? 1 : 0;
        }

        private static int GetRectOriginalOrder(Rect* a, Rect* b)
        {
            return a->WasPacked < b->WasPacked ? -1 : a->WasPacked > b->WasPacked ? 1 : 0;
        }
    }
}
